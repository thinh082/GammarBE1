namespace GammarDomain.Entities;

public class ProductCategory
{
    public long Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<Course> Courses { get; private set; } = [];

    private ProductCategory()
    {
    }

    public ProductCategory(string code, string name, string? description, int sortOrder, bool isActive = true)
    {
        Code = code;
        Name = name;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string code, string name, string? description, int sortOrder, bool isActive)
    {
        Code = code;
        Name = name;
        Description = description;
        SortOrder = sortOrder;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
