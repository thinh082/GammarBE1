namespace GammarDomain.Entities;

public class ProfileCharacter
{
    public long Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Prompt { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ProfileCharacter()
    {
    }

    public ProfileCharacter(string name, string prompt, string? description = null, bool isActive = true)
    {
        Name = name;
        Prompt = prompt;
        Description = description;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
