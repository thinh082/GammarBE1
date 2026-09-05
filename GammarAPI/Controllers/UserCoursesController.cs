using GammarAPI.DTOs.Courses;
using GammarApplication.Interfaces;
using GammarDomain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/Users/{userId:long}/courses")]
public class UserCoursesController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserCourseRepository _userCourseRepository;

    public UserCoursesController(
        IUserRepository userRepository,
        ICourseRepository courseRepository,
        IUserCourseRepository userCourseRepository)
    {
        _userRepository = userRepository;
        _courseRepository = courseRepository;
        _userCourseRepository = userCourseRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(long userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "User not found" });
        }

        var userCourses = await _userCourseRepository.GetByUserIdAsync(userId, cancellationToken);
        return Ok(userCourses.Where(x => x.Course is not null).Select(MapUserCourse).ToList());
    }

    [HttpGet("{courseId:long}")]
    public async Task<IActionResult> GetByCourseId(long userId, long courseId, CancellationToken cancellationToken)
    {
        var userCourse = await _userCourseRepository.GetByUserAndCourseIdAsync(userId, courseId, cancellationToken);
        if (userCourse is null || userCourse.Course is null)
        {
            return NotFound(new { message = "User course not found" });
        }

        return Ok(MapUserCourse(userCourse));
    }

    [HttpPost]
    public async Task<IActionResult> Assign(long userId, [FromBody] AssignUserCourseRequest request, CancellationToken cancellationToken)
    {
        if (request.CourseId <= 0)
        {
            return BadRequest(new { message = "CourseId is required" });
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "User not found" });
        }

        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return NotFound(new { message = "Course not found" });
        }

        var existing = await _userCourseRepository.GetByUserAndCourseIdAsync(userId, request.CourseId, cancellationToken);
        if (existing is not null)
        {
            return Conflict(new { message = "User already has this course" });
        }

        var userCourse = new UserCourse(userId, request.CourseId);
        await _userCourseRepository.AddAsync(userCourse, cancellationToken);
        await _userCourseRepository.SaveChangesAsync(cancellationToken);

        var created = await _userCourseRepository.GetByUserAndCourseIdAsync(userId, request.CourseId, cancellationToken);
        if (created is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load created user course" });
        }

        return Ok(MapUserCourse(created));
    }

    [HttpPatch("{courseId:long}/progress")]
    public async Task<IActionResult> UpdateProgress(long userId, long courseId, [FromBody] UpdateUserCourseProgressRequest request, CancellationToken cancellationToken)
    {
        if (request.ProgressPercent < 0 || request.ProgressPercent > 100)
        {
            return BadRequest(new { message = "ProgressPercent must be between 0 and 100" });
        }

        var userCourse = await _userCourseRepository.GetByUserAndCourseIdAsync(userId, courseId, cancellationToken);
        if (userCourse is null || userCourse.Course is null)
        {
            return NotFound(new { message = "User course not found" });
        }

        userCourse.UpdateProgress(request.ProgressPercent);
        _userCourseRepository.Update(userCourse);
        await _userCourseRepository.SaveChangesAsync(cancellationToken);

        return Ok(MapUserCourse(userCourse));
    }

    private static UserCourseDto MapUserCourse(UserCourse userCourse)
    {
        var course = userCourse.Course ?? throw new InvalidOperationException("UserCourse.Course must be loaded");
        return new UserCourseDto(
            userCourse.Id,
            userCourse.UserId,
            userCourse.CourseId,
            userCourse.Status,
            userCourse.ProgressPercent,
            userCourse.StartedAt,
            userCourse.CompletedAt,
            userCourse.ExpiredAt,
            CoursesController.MapCourseListItem(course));
    }
}
