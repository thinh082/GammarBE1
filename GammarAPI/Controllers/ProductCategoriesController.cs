using GammarAPI.DTOs.Courses;
using GammarApplication.Interfaces;
using GammarDomain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductCategoriesController : ControllerBase
{
    private readonly IProductCategoryRepository _productCategoryRepository;

    public ProductCategoriesController(IProductCategoryRepository productCategoryRepository)
    {
        _productCategoryRepository = productCategoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _productCategoryRepository.GetActiveAsync(cancellationToken);
        return Ok(categories.Select(MapCategory).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Code and name are required" });
        }

        var existing = await _productCategoryRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing is not null)
        {
            return Conflict(new { message = "Category code already exists" });
        }

        var category = new ProductCategory(request.Code.Trim(), request.Name.Trim(), request.Description?.Trim(), request.SortOrder);
        await _productCategoryRepository.AddAsync(category, cancellationToken);
        await _productCategoryRepository.SaveChangesAsync(cancellationToken);

        return Ok(MapCategory(category));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductCategoryRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Code and name are required" });
        }

        var category = await _productCategoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return NotFound(new { message = "Category not found" });
        }

        var duplicate = await _productCategoryRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (duplicate is not null && duplicate.Id != id)
        {
            return Conflict(new { message = "Category code already exists" });
        }

        category.Update(request.Code.Trim(), request.Name.Trim(), request.Description?.Trim(), request.SortOrder, request.IsActive);
        _productCategoryRepository.Update(category);
        await _productCategoryRepository.SaveChangesAsync(cancellationToken);

        return Ok(MapCategory(category));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var category = await _productCategoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return NotFound(new { message = "Category not found" });
        }

        category.Deactivate();
        _productCategoryRepository.Update(category);
        await _productCategoryRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static ProductCategoryDto MapCategory(ProductCategory category)
    {
        return new ProductCategoryDto(
            category.Id,
            category.Code,
            category.Name,
            category.Description,
            category.SortOrder,
            category.IsActive);
    }
}
