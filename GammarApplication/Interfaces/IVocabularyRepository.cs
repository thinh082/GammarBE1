using GammarDomain.Entities;

namespace GammarApplication.Interfaces;

public interface IVocabularyRepository
{
    Task<List<Vocabulary>> GetActiveAsync(
        string? keyword,
        string? levelCode,
        string? categoryCode,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Vocabulary?> GetActiveByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Vocabulary?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<List<long>> GetFavoriteVocabularyIdsAsync(
        long userId,
        IReadOnlyCollection<long> vocabularyIds,
        CancellationToken cancellationToken = default);
    Task AddAsync(Vocabulary vocabulary, CancellationToken cancellationToken = default);
    void Update(Vocabulary vocabulary);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
