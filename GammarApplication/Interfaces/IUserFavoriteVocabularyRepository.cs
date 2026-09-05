using GammarDomain.Entities;

namespace GammarApplication.Interfaces;

public interface IUserFavoriteVocabularyRepository
{
    Task<List<UserFavoriteVocabulary>> GetByUserIdAsync(
        long userId,
        string? keyword,
        string? levelCode,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<UserFavoriteVocabulary?> GetByUserAndVocabularyIdAsync(
        long userId,
        long vocabularyId,
        CancellationToken cancellationToken = default);
    Task AddAsync(UserFavoriteVocabulary favoriteVocabulary, CancellationToken cancellationToken = default);
    void Delete(UserFavoriteVocabulary favoriteVocabulary);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
