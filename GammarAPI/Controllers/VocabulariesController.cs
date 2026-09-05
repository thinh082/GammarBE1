using GammarAPI.DTOs.Vocabularies;
using GammarApplication.Interfaces;
using GammarDomain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VocabulariesController : ControllerBase
{
    private readonly IVocabularyRepository _vocabularyRepository;
    private readonly IUserRepository _userRepository;

    public VocabulariesController(IVocabularyRepository vocabularyRepository, IUserRepository userRepository)
    {
        _vocabularyRepository = vocabularyRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? keyword,
        [FromQuery] string? levelCode,
        [FromQuery] string? categoryCode,
        [FromQuery] long? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        if (userId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user is null)
            {
                return NotFound(new { message = "User not found" });
            }
        }

        var vocabularies = await _vocabularyRepository.GetActiveAsync(
            keyword?.Trim(),
            levelCode?.Trim(),
            categoryCode?.Trim(),
            page,
            pageSize,
            cancellationToken);

        var favoriteIds = await ResolveFavoriteIdsAsync(userId, vocabularies.Select(x => x.Id).ToList(), cancellationToken);
        return Ok(vocabularies.Select(x => MapVocabulary(x, favoriteIds.Contains(x.Id))).ToList());
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, [FromQuery] long? userId, CancellationToken cancellationToken)
    {
        if (userId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user is null)
            {
                return NotFound(new { message = "User not found" });
            }
        }

        var vocabulary = await _vocabularyRepository.GetActiveByIdAsync(id, cancellationToken);
        if (vocabulary is null)
        {
            return NotFound(new { message = "Vocabulary not found" });
        }

        var favoriteIds = await ResolveFavoriteIdsAsync(userId, [vocabulary.Id], cancellationToken);
        return Ok(MapVocabulary(vocabulary, favoriteIds.Contains(vocabulary.Id)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVocabularyRequest request, CancellationToken cancellationToken)
    {
        var validationResult = ValidateVocabularyRequest(request.Kanji, request.MeaningVi);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var vocabulary = new Vocabulary(
            request.Kanji.Trim(),
            request.Kana?.Trim(),
            request.MeaningVi.Trim(),
            request.LevelCode?.Trim(),
            request.CategoryCode?.Trim(),
            request.ExampleText?.Trim(),
            request.ExampleMeaningVi?.Trim(),
            request.SortOrder);

        await _vocabularyRepository.AddAsync(vocabulary, cancellationToken);
        await _vocabularyRepository.SaveChangesAsync(cancellationToken);

        return Ok(MapVocabulary(vocabulary));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateVocabularyRequest request, CancellationToken cancellationToken)
    {
        var validationResult = ValidateVocabularyRequest(request.Kanji, request.MeaningVi);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var vocabulary = await _vocabularyRepository.GetByIdAsync(id, cancellationToken);
        if (vocabulary is null)
        {
            return NotFound(new { message = "Vocabulary not found" });
        }

        vocabulary.Update(
            request.Kanji.Trim(),
            request.Kana?.Trim(),
            request.MeaningVi.Trim(),
            request.LevelCode?.Trim(),
            request.CategoryCode?.Trim(),
            request.ExampleText?.Trim(),
            request.ExampleMeaningVi?.Trim(),
            request.SortOrder,
            request.IsActive);

        _vocabularyRepository.Update(vocabulary);
        await _vocabularyRepository.SaveChangesAsync(cancellationToken);

        return Ok(MapVocabulary(vocabulary));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var vocabulary = await _vocabularyRepository.GetByIdAsync(id, cancellationToken);
        if (vocabulary is null)
        {
            return NotFound(new { message = "Vocabulary not found" });
        }

        vocabulary.Deactivate();
        _vocabularyRepository.Update(vocabulary);
        await _vocabularyRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    internal static VocabularyDto MapVocabulary(Vocabulary vocabulary, bool isFavorite = false)
    {
        return new VocabularyDto(
            vocabulary.Id,
            vocabulary.Kanji,
            vocabulary.Kana,
            vocabulary.MeaningVi,
            vocabulary.LevelCode,
            vocabulary.CategoryCode,
            vocabulary.ExampleText,
            vocabulary.ExampleMeaningVi,
            vocabulary.SortOrder,
            vocabulary.IsActive,
            isFavorite);
    }

    private IActionResult? ValidateVocabularyRequest(string kanji, string meaningVi)
    {
        if (string.IsNullOrWhiteSpace(kanji) || string.IsNullOrWhiteSpace(meaningVi))
        {
            return BadRequest(new { message = "Kanji and meaningVi are required" });
        }

        return null;
    }

    private async Task<HashSet<long>> ResolveFavoriteIdsAsync(
        long? userId,
        IReadOnlyCollection<long> vocabularyIds,
        CancellationToken cancellationToken)
    {
        if (!userId.HasValue || vocabularyIds.Count == 0)
        {
            return [];
        }

        var favoriteIds = await _vocabularyRepository.GetFavoriteVocabularyIdsAsync(
            userId.Value,
            vocabularyIds,
            cancellationToken);

        return favoriteIds.ToHashSet();
    }
}
