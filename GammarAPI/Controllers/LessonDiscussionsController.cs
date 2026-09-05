using GammarAPI.DTOs.Courses;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/lessons/{lessonId:long}/discussions")]
public class LessonDiscussionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public LessonDiscussionsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        long lessonId,
        [FromQuery] long? userId,
        CancellationToken cancellationToken)
    {
        var lessonExists = await _dbContext.Lessons
            .AsNoTracking()
            .AnyAsync(x => x.Id == lessonId, cancellationToken);

        if (!lessonExists)
        {
            return NotFound(new { message = "Lesson not found" });
        }

        var likedDiscussionIds = userId > 0
            ? await _dbContext.LessonDiscussionLikes
                .AsNoTracking()
                .Where(x => x.UserId == userId.Value)
                .Select(x => x.DiscussionId)
                .ToListAsync(cancellationToken)
            : [];

        var likedDiscussionIdSet = likedDiscussionIds.ToHashSet();

        var items = await _dbContext.LessonDiscussions
            .AsNoTracking()
            .Where(x => x.LessonId == lessonId && !x.IsDeleted)
            .Select(x => new DiscussionProjection(
                x.Id,
                x.LessonId,
                x.UserId,
                x.ParentId,
                x.Content,
                x.LikeCount,
                x.ReplyCount,
                x.IsEdited,
                x.IsDeleted,
                x.CreatedAt,
                x.UpdatedAt,
                x.User != null ? x.User.Email : null,
                _dbContext.Profiles
                    .Where(profile => profile.UserId == x.UserId)
                    .Select(profile => profile.FullName)
                    .FirstOrDefault(),
                _dbContext.Profiles
                    .Where(profile => profile.UserId == x.UserId)
                    .Select(profile => profile.AvatarUrl)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        var itemLookup = items
            .OrderByDescending(x => x.CreatedAt)
            .ToDictionary(x => x.Id, x => x);

        var repliesLookup = items
            .Where(x => x.ParentId.HasValue)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(
                x => x.Key,
                x => x.OrderBy(item => item.CreatedAt).ToList());

        LessonDiscussionItemDto MapItem(DiscussionProjection item)
        {
            var displayName = string.IsNullOrWhiteSpace(item.FullName)
                ? (string.IsNullOrWhiteSpace(item.Email) ? $"User {item.UserId}" : item.Email!)
                : item.FullName!;

            var replies = repliesLookup.TryGetValue(item.Id, out var children)
                ? children.Select(MapItem).ToList()
                : [];

            return new LessonDiscussionItemDto(
                item.Id,
                item.LessonId,
                item.UserId,
                item.ParentId,
                item.Content,
                item.LikeCount,
                item.ReplyCount,
                item.IsEdited,
                item.IsDeleted,
                item.CreatedAt,
                item.UpdatedAt,
                likedDiscussionIdSet.Contains(item.Id),
                new LessonDiscussionAuthorDto(
                    item.UserId,
                    displayName,
                    item.AvatarUrl,
                    item.Email),
                replies);
        }

        var results = itemLookup.Values
            .Where(x => !x.ParentId.HasValue)
            .OrderByDescending(x => x.CreatedAt)
            .Select(MapItem)
            .ToList();

        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        long lessonId,
        [FromBody] CreateLessonDiscussionRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateRequest(request);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var lessonExists = await _dbContext.Lessons
            .AnyAsync(x => x.Id == lessonId, cancellationToken);

        if (!lessonExists)
        {
            return NotFound(new { message = "Lesson not found" });
        }

        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return NotFound(new { message = "User not found" });
        }

        var discussion = new LessonDiscussion(lessonId, request.UserId, request.Content);
        await _dbContext.LessonDiscussions.AddAsync(discussion, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await GetDiscussionItemAsync(discussion.Id, request.UserId, cancellationToken);
        if (created is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load created discussion" });
        }

        return Ok(created);
    }

    [HttpPost("{discussionId:long}/replies")]
    public async Task<IActionResult> CreateReply(
        long lessonId,
        long discussionId,
        [FromBody] CreateLessonDiscussionRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateRequest(request);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var parentDiscussion = await _dbContext.LessonDiscussions
            .FirstOrDefaultAsync(
                x => x.Id == discussionId && x.LessonId == lessonId && !x.IsDeleted,
                cancellationToken);

        if (parentDiscussion is null)
        {
            return NotFound(new { message = "Discussion not found" });
        }

        if (parentDiscussion.ParentId.HasValue)
        {
            return BadRequest(new { message = "Only root discussions can receive replies" });
        }

        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return NotFound(new { message = "User not found" });
        }

        var reply = new LessonDiscussion(lessonId, request.UserId, request.Content, discussionId);
        parentDiscussion.IncrementReplyCount();

        await _dbContext.LessonDiscussions.AddAsync(reply, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await GetDiscussionItemAsync(reply.Id, request.UserId, cancellationToken);
        if (created is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load created reply" });
        }

        return Ok(created);
    }

    [HttpPost("{discussionId:long}/likes")]
    public async Task<IActionResult> Like(
        long lessonId,
        long discussionId,
        [FromBody] ToggleLessonDiscussionLikeRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            return BadRequest(new { message = "UserId is required" });
        }

        var discussion = await _dbContext.LessonDiscussions
            .FirstOrDefaultAsync(
                x => x.Id == discussionId && x.LessonId == lessonId && !x.IsDeleted,
                cancellationToken);

        if (discussion is null)
        {
            return NotFound(new { message = "Discussion not found" });
        }

        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return NotFound(new { message = "User not found" });
        }

        var existingLike = await _dbContext.LessonDiscussionLikes
            .FirstOrDefaultAsync(
                x => x.DiscussionId == discussionId && x.UserId == request.UserId,
                cancellationToken);

        if (existingLike is not null)
        {
            return Conflict(new { message = "Discussion already liked by this user" });
        }

        var like = new LessonDiscussionLike(discussionId, request.UserId);
        discussion.IncrementLikeCount();

        await _dbContext.LessonDiscussionLikes.AddAsync(like, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            discussionId,
            likeCount = discussion.LikeCount,
            isLikedByCurrentUser = true,
        });
    }

    [HttpDelete("{discussionId:long}/likes/{userId:long}")]
    public async Task<IActionResult> Unlike(
        long lessonId,
        long discussionId,
        long userId,
        CancellationToken cancellationToken)
    {
        var discussion = await _dbContext.LessonDiscussions
            .FirstOrDefaultAsync(
                x => x.Id == discussionId && x.LessonId == lessonId && !x.IsDeleted,
                cancellationToken);

        if (discussion is null)
        {
            return NotFound(new { message = "Discussion not found" });
        }

        var existingLike = await _dbContext.LessonDiscussionLikes
            .FirstOrDefaultAsync(
                x => x.DiscussionId == discussionId && x.UserId == userId,
                cancellationToken);

        if (existingLike is null)
        {
            return NotFound(new { message = "Like not found" });
        }

        _dbContext.LessonDiscussionLikes.Remove(existingLike);
        discussion.DecrementLikeCount();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            discussionId,
            likeCount = discussion.LikeCount,
            isLikedByCurrentUser = false,
        });
    }

    private async Task<LessonDiscussionItemDto?> GetDiscussionItemAsync(
        long discussionId,
        long? currentUserId,
        CancellationToken cancellationToken)
    {
        var likedDiscussionIds = currentUserId > 0
            ? await _dbContext.LessonDiscussionLikes
                .AsNoTracking()
                .Where(x => x.UserId == currentUserId.Value && x.DiscussionId == discussionId)
                .Select(x => x.DiscussionId)
                .ToListAsync(cancellationToken)
            : [];

        var item = await _dbContext.LessonDiscussions
            .AsNoTracking()
            .Where(x => x.Id == discussionId && !x.IsDeleted)
            .Select(x => new DiscussionProjection(
                x.Id,
                x.LessonId,
                x.UserId,
                x.ParentId,
                x.Content,
                x.LikeCount,
                x.ReplyCount,
                x.IsEdited,
                x.IsDeleted,
                x.CreatedAt,
                x.UpdatedAt,
                x.User != null ? x.User.Email : null,
                _dbContext.Profiles
                    .Where(profile => profile.UserId == x.UserId)
                    .Select(profile => profile.FullName)
                    .FirstOrDefault(),
                _dbContext.Profiles
                    .Where(profile => profile.UserId == x.UserId)
                    .Select(profile => profile.AvatarUrl)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var displayName = string.IsNullOrWhiteSpace(item.FullName)
            ? (string.IsNullOrWhiteSpace(item.Email) ? $"User {item.UserId}" : item.Email!)
            : item.FullName!;

        return new LessonDiscussionItemDto(
            item.Id,
            item.LessonId,
            item.UserId,
            item.ParentId,
            item.Content,
            item.LikeCount,
            item.ReplyCount,
            item.IsEdited,
            item.IsDeleted,
            item.CreatedAt,
            item.UpdatedAt,
            likedDiscussionIds.Contains(item.Id),
            new LessonDiscussionAuthorDto(
                item.UserId,
                displayName,
                item.AvatarUrl,
                item.Email),
            []);
    }

    private static string? ValidateCreateRequest(CreateLessonDiscussionRequest request)
    {
        if (request.UserId <= 0)
        {
            return "UserId is required";
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return "Content is required";
        }

        return request.Content.Trim().Length > 2000
            ? "Content must be at most 2000 characters"
            : null;
    }

    private sealed record DiscussionProjection(
        long Id,
        long LessonId,
        long UserId,
        long? ParentId,
        string Content,
        int LikeCount,
        int ReplyCount,
        bool IsEdited,
        bool IsDeleted,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string? Email,
        string? FullName,
        string? AvatarUrl);
}
