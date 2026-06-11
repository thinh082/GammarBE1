using GammarAPI.DTOs.Users;
using GammarApplication.Interfaces;
using GammarDomain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileCharactersController : ControllerBase
{
    private readonly IGenericRepository<ProfileCharacter> _characterRepository;

    public ProfileCharactersController(IGenericRepository<ProfileCharacter> characterRepository)
    {
        _characterRepository = characterRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var characters = await _characterRepository.GetAllAsync(cancellationToken);
        var active = characters
            .Where(c => c.IsActive)
            .Select(c => new ProfileCharacterDto(c.Id, c.Name, c.Prompt, c.Description))
            .ToList();
        return Ok(active);
    }
}
