namespace GammarDomain.Entities;

public class Course
{
    public long Id { get; private set; }
    public long ProductCategoryId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? ShortDescription { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public string? LevelCode { get; private set; }
    public int? DurationMonths { get; private set; }
    public decimal Price { get; private set; }
    public decimal? OriginalPrice { get; private set; }
    public string Currency { get; private set; } = "VND";
    public bool IsFree { get; private set; }
    public bool IsHot { get; private set; }
    public bool IsPublished { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ProductCategory? ProductCategory { get; private set; }
    public ICollection<Lesson> Lessons { get; private set; } = [];
    public ICollection<UserCourse> UserCourses { get; private set; } = [];

    private Course()
    {
    }

    public Course(
        long productCategoryId,
        string code,
        string slug,
        string title,
        string? shortDescription,
        string? thumbnailUrl,
        string? levelCode,
        int? durationMonths,
        decimal price,
        decimal? originalPrice,
        string currency,
        bool isFree,
        bool isHot,
        bool isPublished,
        int sortOrder)
    {
        ProductCategoryId = productCategoryId;
        Code = code;
        Slug = slug;
        Title = title;
        ShortDescription = shortDescription;
        ThumbnailUrl = thumbnailUrl;
        LevelCode = levelCode;
        DurationMonths = durationMonths;
        Price = price;
        OriginalPrice = originalPrice;
        Currency = currency;
        IsFree = isFree;
        IsHot = isHot;
        IsPublished = isPublished;
        SortOrder = sortOrder;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        long productCategoryId,
        string code,
        string slug,
        string title,
        string? shortDescription,
        string? thumbnailUrl,
        string? levelCode,
        int? durationMonths,
        decimal price,
        decimal? originalPrice,
        string currency,
        bool isFree,
        bool isHot,
        bool isPublished,
        int sortOrder)
    {
        ProductCategoryId = productCategoryId;
        Code = code;
        Slug = slug;
        Title = title;
        ShortDescription = shortDescription;
        ThumbnailUrl = thumbnailUrl;
        LevelCode = levelCode;
        DurationMonths = durationMonths;
        Price = price;
        OriginalPrice = originalPrice;
        Currency = currency;
        IsFree = isFree;
        IsHot = isHot;
        IsPublished = isPublished;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
