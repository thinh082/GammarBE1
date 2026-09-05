using GammarApplication.Interfaces;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Repositories;

public class VocabularyRepository : IVocabularyRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<Vocabulary> _dbSet;

    public VocabularyRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Vocabulary>();
    }

    public Task<List<Vocabulary>> GetActiveAsync(
        string? keyword,
        string? levelCode,
        string? categoryCode,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Kanji, pattern) ||
                (x.Kana != null && EF.Functions.ILike(x.Kana, pattern)) ||
                EF.Functions.ILike(x.MeaningVi, pattern));
        }

        if (!string.IsNullOrWhiteSpace(levelCode))
        {
            query = query.Where(x => x.LevelCode == levelCode);
        }

        if (!string.IsNullOrWhiteSpace(categoryCode))
        {
            query = query.Where(x => x.CategoryCode == categoryCode);
        }

        return query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<Vocabulary?> GetActiveByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);
    }

    public Task<Vocabulary?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<long>> GetFavoriteVocabularyIdsAsync(
        long userId,
        IReadOnlyCollection<long> vocabularyIds,
        CancellationToken cancellationToken = default)
    {
        if (vocabularyIds.Count == 0)
        {
            return Task.FromResult(new List<long>());
        }

        return _context.Set<UserFavoriteVocabulary>()
            .Where(x => x.UserId == userId && vocabularyIds.Contains(x.VocabularyId))
            .Select(x => x.VocabularyId)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Vocabulary vocabulary, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(vocabulary, cancellationToken).AsTask();
    }

    public void Update(Vocabulary vocabulary)
    {
        _dbSet.Update(vocabulary);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
