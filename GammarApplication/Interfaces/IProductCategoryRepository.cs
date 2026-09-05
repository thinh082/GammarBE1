using GammarDomain.Entities;

namespace GammarApplication.Interfaces;

public interface IProductCategoryRepository
{
    Task<List<ProductCategory>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ProductCategory?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ProductCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(ProductCategory category, CancellationToken cancellationToken = default);
    void Update(ProductCategory category);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
