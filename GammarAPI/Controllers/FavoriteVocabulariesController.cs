using GammarAPI.DTOs.Vocabularies;
using GammarApplication.Interfaces;
using GammarDomain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/Users/{userId:long}/favorite-vocabularies")]
public class FavoriteVocabulariesController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IVocabularyRepository _vocabularyRepository;
    private readonly IUserFavoriteVocabularyRepository _userFavoriteVocabularyRepository;

    public FavoriteVocabulariesController(
        IUserRepository userRepository,
        IVocabularyRepository vocabularyRepository,
        IUserFavoriteVocabularyRepository userFavoriteVocabularyRepository)
    {
        _userRepository = userRepository;
        _vocabularyRepository = vocabularyRepository;
        _userFavoriteVocabularyRepository = userFavoriteVocabularyRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        long userId,
        [FromQuery] string? keyword,
        [FromQuery] string? levelCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "User not found" });
        }

        var favorites = await _userFavoriteVocabularyRepository.GetByUserIdAsync(
            userId,
            keyword?.Trim(),
            levelCode?.Trim(),
            page,
            pageSize,
            cancellationToken);

        return Ok(favorites
            .Where(x => x.Vocabulary is not null)
            .Select(MapFavorite)
            .ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        long userId,
        [FromBody] AssignUserFavoriteVocabularyRequest request,
        CancellationToken cancellationToken)
    {
        if (request.VocabularyId <= 0)
        {
            return BadRequest(new { message = "VocabularyId is required" });
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "User not found" });
        }

        var vocabulary = await _vocabularyRepository.GetActiveByIdAsync(request.VocabularyId, cancellationToken);
        if (vocabulary is null)
        {
            return NotFound(new { message = "Vocabulary not found" });
        }

        var existing = await _userFavoriteVocabularyRepository.GetByUserAndVocabularyIdAsync(
            userId,
            request.VocabularyId,
            cancellationToken);

        if (existing is not null)
        {
            return Conflict(new { message = "Vocabulary already added to favorites" });
        }

        var favorite = new UserFavoriteVocabulary(userId, request.VocabularyId);
        await _userFavoriteVocabularyRepository.AddAsync(favorite, cancellationToken);
        await _userFavoriteVocabularyRepository.SaveChangesAsync(cancellationToken);

        var created = await _userFavoriteVocabularyRepository.GetByUserAndVocabularyIdAsync(
            userId,
            request.VocabularyId,
            cancellationToken);

        if (created is null || created.Vocabulary is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load created favorite vocabulary" });
        }

        return Ok(MapFavorite(created));
    }

    [HttpDelete("{vocabularyId:long}")]
    public async Task<IActionResult> Delete(long userId, long vocabularyId, CancellationToken cancellationToken)
    {
        var favorite = await _userFavoriteVocabularyRepository.GetByUserAndVocabularyIdAsync(userId, vocabularyId, cancellationToken);
        if (favorite is null)
        {
            return NotFound(new { message = "Favorite vocabulary not found" });
        }

        _userFavoriteVocabularyRepository.Delete(favorite);
        await _userFavoriteVocabularyRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static UserFavoriteVocabularyDto MapFavorite(UserFavoriteVocabulary favorite)
    {
        var vocabulary = favorite.Vocabulary ?? throw new InvalidOperationException("Favorite vocabulary must include Vocabulary");
        return new UserFavoriteVocabularyDto(
            favorite.Id,
            favorite.UserId,
            favorite.VocabularyId,
            favorite.CreatedAt,
            VocabulariesController.MapVocabulary(vocabulary, true));
    }
}
