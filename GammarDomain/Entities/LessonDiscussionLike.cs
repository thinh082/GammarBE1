namespace GammarDomain.Entities;

public class LessonDiscussionLike
{
    public long Id { get; private set; }
    public long DiscussionId { get; private set; }
    public long UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public LessonDiscussion? Discussion { get; private set; }
    public User? User { get; private set; }

    private LessonDiscussionLike()
    {
    }

    public LessonDiscussionLike(long discussionId, long userId)
    {
        DiscussionId = discussionId;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }
}
