using GammarAPI.DTOs.Courses;
using GammarApplication.Interfaces;
using GammarDomain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseRepository _courseRepository;
    private readonly IProductCategoryRepository _productCategoryRepository;

    public CoursesController(ICourseRepository courseRepository, IProductCategoryRepository productCategoryRepository)
    {
        _courseRepository = courseRepository;
        _productCategoryRepository = productCategoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? keyword,
        [FromQuery] string? categoryCode,
        [FromQuery] string? levelCode,
        [FromQuery] bool? isFree,
        [FromQuery] bool? isHot,
        [FromQuery] string? isPublished,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var publicationFilter = ResolvePublicationFilter(isPublished);

        if (!publicationFilter.IsValid)
        {
            return BadRequest(new { message = "isPublished must be true, false or empty" });
        }

        var courses = await _courseRepository.GetFilteredAsync(
            keyword?.Trim(),
            categoryCode?.Trim(),
            levelCode?.Trim(),
            isFree,
            isHot,
            publicationFilter.Value,
            page,
            pageSize,
            cancellationToken);

        return Ok(courses.Select(MapCourseListItem).ToList());
    }

    private (bool IsValid, bool? Value) ResolvePublicationFilter(string? isPublished)
    {
        if (!Request.Query.ContainsKey("isPublished"))
        {
            return (true, true);
        }

        if (string.IsNullOrWhiteSpace(isPublished))
        {
            return (true, null);
        }

        if (bool.TryParse(isPublished, out var parsedValue))
        {
            return (true, parsedValue);
        }

        return (false, null);
    }

    [HttpGet("{courseId:long}")]
    public async Task<IActionResult> GetById(long courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetPublishedByIdAsync(courseId, cancellationToken);
        if (course is null || course.ProductCategory is null)
        {
            return NotFound(new { message = "Course not found" });
        }

        return Ok(MapCourseDetail(course));
    }

    [HttpGet("{courseId:long}/lessons")]
    public async Task<IActionResult> GetLessons(long courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetPublishedByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            return NotFound(new { message = "Course not found" });
        }

        var lessons = await _courseRepository.GetPublishedLessonsAsync(courseId, cancellationToken);
        return Ok(lessons.Select(MapCourseLessonItem).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateCourseRequestAsync(
            request.ProductCategoryId,
            request.Code,
            request.Slug,
            request.Title,
            request.Price,
            request.OriginalPrice,
            null,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var course = new Course(
            request.ProductCategoryId,
            request.Code.Trim(),
            request.Slug.Trim(),
            request.Title.Trim(),
            request.ShortDescription?.Trim(),
            request.ThumbnailUrl?.Trim(),
            request.LevelCode?.Trim(),
            request.DurationMonths,
            request.Price,
            request.OriginalPrice,
            string.IsNullOrWhiteSpace(request.Currency) ? "VND" : request.Currency.Trim(),
            request.IsFree,
            request.IsHot,
            request.IsPublished,
            request.SortOrder);

        await _courseRepository.AddAsync(course, cancellationToken);
        await _courseRepository.SaveChangesAsync(cancellationToken);

        var created = await _courseRepository.GetByIdAsync(course.Id, cancellationToken);
        return Ok(MapCourseDetail(created ?? course));
    }

    [HttpPut("{courseId:long}")]
    public async Task<IActionResult> Update(long courseId, [FromBody] UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            return NotFound(new { message = "Course not found" });
        }

        var validationResult = await ValidateCourseRequestAsync(
            request.ProductCategoryId,
            request.Code,
            request.Slug,
            request.Title,
            request.Price,
            request.OriginalPrice,
            courseId,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        course.Update(
            request.ProductCategoryId,
            request.Code.Trim(),
            request.Slug.Trim(),
            request.Title.Trim(),
            request.ShortDescription?.Trim(),
            request.ThumbnailUrl?.Trim(),
            request.LevelCode?.Trim(),
            request.DurationMonths,
            request.Price,
            request.OriginalPrice,
            string.IsNullOrWhiteSpace(request.Currency) ? "VND" : request.Currency.Trim(),
            request.IsFree,
            request.IsHot,
            request.IsPublished,
            request.SortOrder);

        _courseRepository.Update(course);
        await _courseRepository.SaveChangesAsync(cancellationToken);

        var updated = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
        return Ok(MapCourseDetail(updated ?? course));
    }

    [HttpDelete("{courseId:long}")]
    public async Task<IActionResult> Delete(long courseId, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            return NotFound(new { message = "Course not found" });
        }

        course.Unpublish();
        _courseRepository.Update(course);
        await _courseRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateCourseRequestAsync(
        long productCategoryId,
        string code,
        string slug,
        string title,
        decimal price,
        decimal? originalPrice,
        long? currentCourseId,
        CancellationToken cancellationToken)
    {
        if (productCategoryId <= 0 ||
            string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(slug) ||
            string.IsNullOrWhiteSpace(title))
        {
            return BadRequest(new { message = "ProductCategoryId, code, slug, title and price are required" });
        }

        if (price < 0 || (originalPrice.HasValue && originalPrice.Value < 0))
        {
            return BadRequest(new { message = "Price values must be greater than or equal to zero" });
        }

        var category = await _productCategoryRepository.GetByIdAsync(productCategoryId, cancellationToken);
        if (category is null)
        {
            return NotFound(new { message = "Product category not found" });
        }

        var duplicateCode = await _courseRepository.GetByCodeAsync(code.Trim(), cancellationToken);
        if (duplicateCode is not null && duplicateCode.Id != currentCourseId)
        {
            return Conflict(new { message = "Course code already exists" });
        }

        var duplicateSlug = await _courseRepository.GetBySlugAsync(slug.Trim(), cancellationToken);
        if (duplicateSlug is not null && duplicateSlug.Id != currentCourseId)
        {
            return Conflict(new { message = "Course slug already exists" });
        }

        return null;
    }

    internal static CourseListItemDto MapCourseListItem(Course course)
    {
        return new CourseListItemDto(
            course.Id,
            course.ProductCategoryId,
            course.ProductCategory?.Code ?? string.Empty,
            course.ProductCategory?.Name ?? string.Empty,
            course.Code,
            course.Slug,
            course.Title,
            course.ShortDescription,
            course.ThumbnailUrl,
            course.LevelCode,
            course.DurationMonths,
            course.Price,
            course.OriginalPrice,
            course.Currency,
            course.IsFree,
            course.IsHot,
            course.IsPublished,
            course.SortOrder);
    }

    private static CourseDetailDto MapCourseDetail(Course course)
    {
        return new CourseDetailDto(
            course.Id,
            course.ProductCategoryId,
            course.ProductCategory?.Code ?? string.Empty,
            course.ProductCategory?.Name ?? string.Empty,
            course.Code,
            course.Slug,
            course.Title,
            course.ShortDescription,
            course.ThumbnailUrl,
            course.LevelCode,
            course.DurationMonths,
            course.Price,
            course.OriginalPrice,
            course.Currency,
            course.IsFree,
            course.IsHot,
            course.IsPublished,
            course.SortOrder,
            course.Lessons.Count(x => x.IsActive));
    }

    private static CourseLessonItemDto MapCourseLessonItem(Lesson lesson)
    {
        return new CourseLessonItemDto(
            lesson.Id,
            lesson.Code,
            lesson.Title,
            lesson.LessonKind,
            lesson.ShortDescription,
            lesson.SortOrder,
            lesson.IsPreview,
            lesson.IsActive);
    }
}
