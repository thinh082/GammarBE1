namespace GammarDomain.Entities;

public class Profile
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public long ProfileCharacterId { get; private set; }
    public string? FullName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    public DateOnly? Birthday { get; private set; }
    public string? Gender { get; private set; }
    public string? Location { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Profile()
    {
    }

    public Profile(long userId, long profileCharacterId, string? fullName = null, string? avatarUrl = null, string? bio = null, DateOnly? birthday = null, string? gender = null, string? location = null)
    {
        UserId = userId;
        ProfileCharacterId = profileCharacterId;
        FullName = fullName;
        AvatarUrl = avatarUrl;
        Bio = bio;
        Birthday = birthday;
        Gender = gender;
        Location = location;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(long profileCharacterId, string? fullName, string? avatarUrl, string? bio, DateOnly? birthday, string? gender, string? location)
    {
        ProfileCharacterId = profileCharacterId;
        FullName = fullName;
        AvatarUrl = avatarUrl;
        Bio = bio;
        Birthday = birthday;
        Gender = gender;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }
}
