using GammarApplication.Interfaces;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Repositories;

public class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<ProductCategory> _dbSet;

    public ProductCategoryRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ProductCategory>();
    }

    public Task<List<ProductCategory>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<ProductCategory?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<ProductCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }

    public Task AddAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(category, cancellationToken).AsTask();
    }

    public void Update(ProductCategory category)
    {
        _dbSet.Update(category);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
