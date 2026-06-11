using GammarAPI.DTOs.Users;
using GammarApplication.Interfaces;
using GammarDomain.Entities;
using GammarInfrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IGenericRepository<ProfileCharacter> _characterRepository;

    public UsersController(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IPasswordHasherService passwordHasherService,
        IGenericRepository<ProfileCharacter> characterRepository)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _passwordHasherService = passwordHasherService;
        _characterRepository = characterRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            return Conflict(new { message = "Email already exists" });
        }

        var user = new User(request.Email, _passwordHasherService.Hash(request.Password), request.Phone);
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var defaultCharacterId = 1L;
        var profile = new Profile(user.Id, defaultCharacterId, fullName: request.FullName);
        await _profileRepository.AddAsync(profile, cancellationToken);
        await _profileRepository.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            UserId = user.Id,
            user.Email,
            FullName = profile.FullName,
            ProfileId = profile.Id,
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        if (!_passwordHasherService.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var profile = await _profileRepository.GetByUserIdAsync(user.Id, cancellationToken);

        return Ok(new
        {
            UserId = user.Id,
            user.Email,
            FullName = profile?.FullName,
            ProfileId = profile?.Id,
        });
    }

    [HttpGet("{userId:long}/profile")]
    public async Task<IActionResult> GetProfile(long userId, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
            return NotFound(new { message = "Profile not found" });

        var character = await _characterRepository.GetByIdAsync(profile.ProfileCharacterId, cancellationToken);
        if (character is null)
            return NotFound(new { message = "Profile character not found" });

        return Ok(new ProfileDetailDto(
            profile.Id,
            profile.UserId,
            profile.ProfileCharacterId,
            profile.FullName,
            profile.AvatarUrl,
            profile.Bio,
            profile.Birthday,
            profile.Gender,
            profile.Location,
            new ProfileCharacterDto(character.Id, character.Name, character.Prompt, character.Description)));
    }

    [HttpPut("{userId:long}/profile")]
    public async Task<IActionResult> UpdateProfile(long userId, [FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return NotFound(new { message = "Profile not found" });
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "User not found" });
        }

        profile.Update(
            request.ProfileCharacterId,
            request.FullName,
            request.AvatarUrl,
            request.Bio,
            request.Birthday,
            request.Gender,
            request.Location);

        _profileRepository.Update(profile);
        await _profileRepository.SaveChangesAsync(cancellationToken);

        return Ok(new ProfileDto(
            profile.Id,
            profile.UserId,
            profile.ProfileCharacterId,
            profile.FullName,
            profile.AvatarUrl,
            profile.Bio,
            profile.Birthday,
            profile.Gender,
            profile.Location));
    }
}
