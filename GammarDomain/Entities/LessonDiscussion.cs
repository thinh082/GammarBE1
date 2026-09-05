namespace GammarDomain.Entities;

public class LessonDiscussion
{
    public long Id { get; private set; }
    public long LessonId { get; private set; }
    public long UserId { get; private set; }
    public long? ParentId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public int LikeCount { get; private set; }
    public int ReplyCount { get; private set; }
    public bool IsEdited { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Lesson? Lesson { get; private set; }
    public User? User { get; private set; }
    public LessonDiscussion? Parent { get; private set; }
    public ICollection<LessonDiscussion> Replies { get; private set; } = [];
    public ICollection<LessonDiscussionLike> Likes { get; private set; } = [];

    private LessonDiscussion()
    {
    }

    public LessonDiscussion(long lessonId, long userId, string content, long? parentId = null)
    {
        LessonId = lessonId;
        UserId = userId;
        ParentId = parentId;
        Content = NormalizeContent(content);
        LikeCount = 0;
        ReplyCount = 0;
        IsEdited = false;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementLikeCount()
    {
        LikeCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementLikeCount()
    {
        LikeCount = Math.Max(0, LikeCount - 1);
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementReplyCount()
    {
        ReplyCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementReplyCount()
    {
        ReplyCount = Math.Max(0, ReplyCount - 1);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeContent(string content)
    {
        return string.IsNullOrWhiteSpace(content)
            ? string.Empty
            : content.Trim();
    }
}
