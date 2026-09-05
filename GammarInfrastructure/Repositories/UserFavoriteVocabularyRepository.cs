using GammarApplication.Interfaces;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Repositories;

public class UserFavoriteVocabularyRepository : IUserFavoriteVocabularyRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<UserFavoriteVocabulary> _dbSet;

    public UserFavoriteVocabularyRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<UserFavoriteVocabulary>();
    }

    public Task<List<UserFavoriteVocabulary>> GetByUserIdAsync(
        long userId,
        string? keyword,
        string? levelCode,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(x => x.Vocabulary)
            .Where(x => x.UserId == userId && x.Vocabulary != null && x.Vocabulary.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Vocabulary!.Kanji, pattern) ||
                (x.Vocabulary!.Kana != null && EF.Functions.ILike(x.Vocabulary.Kana, pattern)) ||
                EF.Functions.ILike(x.Vocabulary!.MeaningVi, pattern));
        }

        if (!string.IsNullOrWhiteSpace(levelCode))
        {
            query = query.Where(x => x.Vocabulary!.LevelCode == levelCode);
        }

        return query
            .OrderBy(x => x.Vocabulary!.SortOrder)
            .ThenBy(x => x.Vocabulary!.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<UserFavoriteVocabulary?> GetByUserAndVocabularyIdAsync(
        long userId,
        long vocabularyId,
        CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Include(x => x.Vocabulary)
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.VocabularyId == vocabularyId,
                cancellationToken);
    }

    public Task AddAsync(UserFavoriteVocabulary favoriteVocabulary, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(favoriteVocabulary, cancellationToken).AsTask();
    }

    public void Delete(UserFavoriteVocabulary favoriteVocabulary)
    {
        _dbSet.Remove(favoriteVocabulary);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
