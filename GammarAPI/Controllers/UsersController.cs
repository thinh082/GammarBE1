using GammarAPI.DTOs.Users;
using GammarApplication.DTOs.Users;
using GammarApplication.Interfaces;
using GammarApplication.Interfaces.Notifications;
using GammarDomain.Entities;
using GammarInfrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive",
        "blocked",
    };

    private readonly IUserRepository _userRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IGenericRepository<ProfileCharacter> _characterRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public UsersController(
        IUserRepository userRepository,
        IProfileRepository profileRepository,
        IPasswordHasherService passwordHasherService,
        IGenericRepository<ProfileCharacter> characterRepository,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _passwordHasherService = passwordHasherService;
        _characterRepository = characterRepository;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    [HttpGet("admin")]
    public async Task<IActionResult> GetAdminUsers(
        [FromQuery] string? keyword,
        [FromQuery] string? status,
        [FromQuery] string? gender,
        [FromQuery] long? profileCharacterId,
        [FromQuery] bool? hasPhone,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await _userRepository.GetAdminUsersAsync(
            keyword?.Trim(),
            status?.Trim(),
            gender?.Trim(),
            profileCharacterId,
            hasPhone,
            page,
            pageSize,
            cancellationToken);

        return Ok(users.Select(MapAdminUserListItem).ToList());
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

        // Auto trigger Welcome Notification
        try
        {
            string displayName = !string.IsNullOrWhiteSpace(profile.FullName) ? profile.FullName : user.Email;
            await _notificationService.SendNotificationAsync(
                user.Id,
                "Chào mừng đến với Gammar!",
                $"Chào mừng {displayName} đến với nền tảng học tiếng Nhật Gammar. Hãy bắt đầu bằng bài test đánh giá trình độ đầu vào để nhận lộ trình học cá nhân hóa nhé!",
                "system",
                "/auth/assessment",
                cancellationToken);
        }
        catch
        {
            // Ignore error so registration succeeds
        }

        if (request.SendWelcomeEmail == true)
        {
            try
            {
                string displayName = !string.IsNullOrWhiteSpace(profile.FullName) ? profile.FullName : user.Email;
                var subject = "Chào mừng bạn đến với Gammar Tiếng Nhật!";
                var body = $@"
<div style=""font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; background-color: #f8fafc; padding: 40px 20px;"">
  <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 24px; overflow: hidden; box-shadow: 0 10px 30px rgba(15, 23, 42, 0.05); border: 1px solid #e2e8f0;"">
    
    <div style=""background: linear-gradient(135deg, #0f766e 0%, #115e59 100%); padding: 40px 30px; text-align: center; color: #ffffff;"">
      <div style=""font-size: 32px; font-weight: 900; letter-spacing: -0.05em; text-transform: uppercase;"">GAMMAR</div>
      <div style=""font-size: 16px; font-weight: 600; color: #ccfbf1; margin-top: 5px; text-transform: uppercase; letter-spacing: 0.15em;"">Học tiếng Nhật hiệu quả mỗi ngày</div>
    </div>

    <div style=""text-align: center; padding: 25px 0 0 0; background-color: #f0fdf4;"">
      <img src=""https://res.cloudinary.com/dbim9avit/image/upload/v1784341956/prompt1_qbgfre.png"" alt=""Chào mừng bạn đến với Gammar"" style=""width: 90%; max-width: 500px; border-radius: 16px; box-shadow: 0 8px 20px rgba(15, 23, 42, 0.1);"" />
    </div>

    <div style=""padding: 40px 35px; color: #334155; line-height: 1.8; font-size: 15px;"">
      <h2 style=""color: #0f766e; font-size: 22px; font-weight: 800; margin-top: 0; margin-bottom: 15px;"">Kính chào {displayName},</h2>
      <p style=""margin: 0 0 20px 0;"">Chào mừng bạn đã gia nhập cộng đồng học tiếng Nhật <strong>Gammar</strong>! Chúng tôi rất vui mừng được đồng hành cùng bạn trên con đường chinh phục tiếng Nhật từ Sơ cấp đến Thượng cấp.</p>
      
      <p style=""margin: 0 0 20px 0;"">Tại <strong>Gammar</strong>, bạn sẽ được trải nghiệm hệ sinh thái học tập tiếng Nhật số 1 dành cho người Việt với:</p>
      
      <div style=""background-color: #f8fafc; border-radius: 16px; padding: 20px; margin-bottom: 30px; border: 1px solid #f1f5f9;"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
          <tr>
            <td valign=""top"" style=""font-size: 20px; padding-right: 12px; color: #10b981;"">✦</td>
            <td style=""font-size: 14px; color: #475569; padding-bottom: 12px;""><strong>Lộ trình học cá nhân hóa:</strong> Bài giảng N5 - N1 được tinh chỉnh gọn gàng, học đến đâu thực hành đến đó.</td>
          </tr>
          <tr>
            <td valign=""top"" style=""font-size: 20px; padding-right: 12px; color: #10b981;"">✦</td>
            <td style=""font-size: 14px; color: #475569; padding-bottom: 12px;""><strong>Luyện nói phản xạ AI (AI Speaking):</strong> Giao tiếp 1-1 với giáo viên ảo AI, sửa lỗi phát âm và phản xạ tức thời.</td>
          </tr>
          <tr>
            <td valign=""top"" style=""font-size: 20px; padding-right: 12px; color: #10b981;"">✦</td>
            <td style=""font-size: 14px; color: #475569; padding-bottom: 12px;""><strong>Thi thử JLPT Mock Exam:</strong> Bộ đề thi chuẩn hóa bám sát cấu trúc đề thi thật giúp đánh giá năng lực chính xác.</td>
          </tr>
          <tr>
            <td valign=""top"" style=""font-size: 20px; padding-right: 12px; color: #10b981;"">✦</td>
            <td style=""font-size: 14px; color: #475569;""><strong>Kho từ vựng & Hán tự:</strong> Hệ thống lưu trữ từ vựng thông minh, ôn tập qua flashcard dễ học dễ nhớ.</td>
          </tr>
        </table>
      </div>

      <div style=""text-align: center; margin: 30px 0;"">
        <img src=""https://res.cloudinary.com/dbim9avit/image/upload/v1784341941/loadmacdinh_cizddm.png"" alt=""Mascot Gammar"" style=""width: 100%; max-width: 480px; border-radius: 16px;"" />
      </div>

      <div style=""text-align: center; margin-top: 30px; margin-bottom: 20px;"">
        <a href=""https://gammar.edu.vn"" style=""background-color: #ff730a; color: #ffffff; text-decoration: none; padding: 14px 35px; border-radius: 50px; font-size: 15px; font-weight: bold; display: inline-block; box-shadow: 0 10px 20px rgba(255, 115, 10, 0.25);"">Bắt Đầu Học Ngay</a>
      </div>

      <p style=""margin: 30px 0 0 0; font-size: 14px; color: #64748b;"">Chúc bạn có những giờ học bổ ích và thú vị tại Gammar!</p>
      <p style=""margin: 5px 0 0 0; font-size: 14px; font-weight: bold; color: #0f766e;"">Đội ngũ Gammar Tiếng Nhật</p>
    </div>

    <div style=""background-color: #f1f5f9; padding: 25px 30px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0;"">
      <p style=""margin: 0 0 10px 0;"">Email này được gửi tự động từ hệ thống Gammar.</p>
      <p style=""margin: 0;"">© 2026 Gammar. Mọi quyền được bảo lưu.</p>
    </div>

  </div>
</div>";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch
            {
                // Ignore welcome email sending error so registration still completes
            }
        }

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

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return NotFound(new { message = "User not found" });

        var character = await _characterRepository.GetByIdAsync(profile.ProfileCharacterId, cancellationToken);
        if (character is null)
            return NotFound(new { message = "Profile character not found" });

        return Ok(new ProfileDetailDto(
            profile.Id,
            profile.UserId,
            profile.ProfileCharacterId,
            profile.FullName,
            user.Phone,
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

        user.UpdatePhone(request.Phone?.Trim());
        _userRepository.Update(user);

        profile.Update(
            request.ProfileCharacterId,
            request.FullName?.Trim(),
            request.AvatarUrl?.Trim(),
            request.Bio?.Trim(),
            request.Birthday,
            request.Gender?.Trim(),
            request.Location?.Trim());

        _profileRepository.Update(profile);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Ok(new ProfileDto(
            profile.Id,
            profile.UserId,
            profile.ProfileCharacterId,
            profile.FullName,
            user.Phone,
            profile.AvatarUrl,
            profile.Bio,
            profile.Birthday,
            profile.Gender,
            profile.Location));
    }

    [HttpPut("{userId:long}/change-password")]
    public async Task<IActionResult> ChangePassword(long userId, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { message = "Mật khẩu hiện tại và mật khẩu mới không được để trống" });
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (!_passwordHasherService.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Mật khẩu hiện tại không chính xác" });
        }

        user.UpdatePasswordHash(_passwordHasherService.Hash(request.NewPassword));
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Thay đổi mật khẩu thành công" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Email không được để trống" });
        }

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null)
        {
            return BadRequest(new { message = "Không tìm thấy tài khoản với email này" });
        }

        var temporaryPassword = Guid.NewGuid().ToString("N").Substring(0, 8);

        user.UpdatePasswordHash(_passwordHasherService.Hash(temporaryPassword));
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        try
        {
            var subject = "Yêu cầu khôi phục mật khẩu - Gammar Tiếng Nhật";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                    <h2 style='color: #0f766e;'>Gammar Tiếng Nhật</h2>
                    <p>Chào bạn,</p>
                    <p>Chúng tôi đã nhận được yêu cầu cấp lại mật khẩu của bạn.</p>
                    <p>Mật khẩu tạm thời mới của bạn là:</p>
                    <div style='background-color: #f1f5f9; padding: 15px; border-radius: 8px; font-size: 20px; font-weight: bold; letter-spacing: 2px; text-align: center; color: #ff730a; margin: 20px 0;'>
                        {temporaryPassword}
                    </div>
                    <p style='color: #64748b; font-size: 13px;'>Vui lòng sử dụng mật khẩu tạm thời này để đăng nhập, sau đó đổi lại mật khẩu mới trong phần cài đặt tài khoản của bạn.</p>
                    <hr style='border: 0; border-top: 1px solid #edf2f7; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #94a3b8;'>Đây là email tự động, vui lòng không phản hồi lại email này.</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, subject, body);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Không thể gửi email khôi phục mật khẩu. Lỗi: " + ex.Message });
        }

        return Ok(new { message = "Mật khẩu tạm thời đã được gửi tới email của bạn. Vui lòng kiểm tra hộp thư." });
    }

    [HttpPut("{userId:long}/admin")]
    public async Task<IActionResult> UpdateAdminUser(long userId, [FromBody] UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        if (request.ProfileCharacterId <= 0)
        {
            return BadRequest(new { message = "ProfileCharacterId is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Status) || !AllowedStatuses.Contains(request.Status.Trim()))
        {
            return BadRequest(new { message = "Status must be active, inactive or blocked" });
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "User not found" });
        }

        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return NotFound(new { message = "Profile not found" });
        }

        var character = await _characterRepository.GetByIdAsync(request.ProfileCharacterId, cancellationToken);
        if (character is null)
        {
            return NotFound(new { message = "Profile character not found" });
        }

        user.UpdatePhone(request.Phone?.Trim());
        user.UpdateStatus(request.Status.Trim().ToLowerInvariant());
        _userRepository.Update(user);

        profile.Update(
            request.ProfileCharacterId,
            request.FullName?.Trim(),
            request.AvatarUrl?.Trim(),
            request.Bio?.Trim(),
            request.Birthday,
            request.Gender?.Trim(),
            request.Location?.Trim());

        _profileRepository.Update(profile);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Ok(MapAdminUserListItem(new AdminUserListItemDto(
            user.Id,
            user.Email,
            user.Phone,
            user.Status,
            user.CreatedAt,
            user.UpdatedAt,
            profile.Id,
            profile.FullName,
            profile.AvatarUrl,
            profile.Gender,
            profile.Location,
            profile.ProfileCharacterId,
            character.Name)));
    }

    private static AdminUserListItemResponse MapAdminUserListItem(AdminUserListItemDto user)
    {
        return new AdminUserListItemResponse(
            user.Id,
            user.Email,
            user.Phone,
            user.Status,
            user.CreatedAt,
            user.UpdatedAt,
            user.ProfileId,
            user.FullName,
            user.AvatarUrl,
            user.Gender,
            user.Location,
            user.ProfileCharacterId,
            user.ProfileCharacterName);
    }
}
