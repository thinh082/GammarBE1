using GammarApplication.Interfaces;
using GammarApplication.Interfaces.Admin;
using GammarApplication.Interfaces.Payments;
using GammarInfrastructure.Persistence;
using GammarInfrastructure.Repositories;
using GammarInfrastructure.Services;
using GammarInfrastructure.Services.Admin;
using GammarInfrastructure.Services.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GammarInfrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IUserCourseRepository, UserCourseRepository>();
        services.AddScoped<IVocabularyRepository, VocabularyRepository>();
        services.AddScoped<IUserFavoriteVocabularyRepository, UserFavoriteVocabularyRepository>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IAdminManagementService, AdminManagementService>();
        services.AddScoped<IAdminReportsService, AdminReportsService>();
        services.AddScoped<IAdminStatisticsService, AdminStatisticsService>();
        services.AddScoped<GammarApplication.Interfaces.Notifications.INotificationService, GammarInfrastructure.Services.Notifications.NotificationService>();
        services.AddScoped<GammarApplication.Interfaces.MockExams.IMockExamService, GammarInfrastructure.Services.MockExams.MockExamService>();
        services.AddScoped<GammarApplication.Interfaces.Assessment.IAssessmentService, GammarInfrastructure.Services.Assessment.AssessmentService>();
        services.AddScoped<IVnPayPaymentService, VnPayPaymentService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
