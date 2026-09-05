namespace GammarDomain.Entities;

public class UserFavoriteVocabulary
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public long VocabularyId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User? User { get; private set; }
    public Vocabulary? Vocabulary { get; private set; }

    private UserFavoriteVocabulary()
    {
    }

    public UserFavoriteVocabulary(long userId, long vocabularyId)
    {
        UserId = userId;
        VocabularyId = vocabularyId;
        CreatedAt = DateTime.UtcNow;
    }
}
