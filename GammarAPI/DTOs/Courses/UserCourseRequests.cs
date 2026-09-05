namespace GammarAPI.DTOs.Courses;

public sealed record AssignUserCourseRequest(long CourseId);

public sealed record UpdateUserCourseProgressRequest(decimal ProgressPercent);
