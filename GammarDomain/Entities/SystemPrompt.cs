namespace GammarDomain.Entities;

public class SystemPrompt
{
    public long Id { get; private set; }
    public string NoiDungPrompt { get; private set; } = string.Empty;
    public string? Name { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SystemPrompt()
    {
    }

    public SystemPrompt(string noiDungPrompt, string? name = null, bool isDefault = false)
    {
        NoiDungPrompt = noiDungPrompt;
        Name = name;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
