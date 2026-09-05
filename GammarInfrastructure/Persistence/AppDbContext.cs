using GammarDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<ProfileCharacter> ProfileCharacters => Set<ProfileCharacter>();
    public DbSet<SystemPrompt> SystemPrompts => Set<SystemPrompt>();
    public DbSet<ApiSetting> ApiSettings => Set<ApiSetting>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<UserCourse> UserCourses => Set<UserCourse>();
    public DbSet<UserLessonProgress> UserLessonProgresses => Set<UserLessonProgress>();
    public DbSet<CourseOrder> CourseOrders => Set<CourseOrder>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<LessionVideo> LessionVideos => Set<LessionVideo>();
    public DbSet<LessionText> LessionTexts => Set<LessionText>();
    public DbSet<LessonQuiz> LessonQuizzes => Set<LessonQuiz>();
    public DbSet<LessonQuizQuestion> LessonQuizQuestions => Set<LessonQuizQuestion>();
    public DbSet<LessonQuizOption> LessonQuizOptions => Set<LessonQuizOption>();
    public DbSet<LessonDiscussion> LessonDiscussions => Set<LessonDiscussion>();
    public DbSet<LessonDiscussionLike> LessonDiscussionLikes => Set<LessonDiscussionLike>();
    public DbSet<Vocabulary> Vocabularies => Set<Vocabulary>();
    public DbSet<UserFavoriteVocabulary> UserFavoriteVocabularies => Set<UserFavoriteVocabulary>();

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<MockExam> MockExams => Set<MockExam>();
    public DbSet<MockExamSection> MockExamSections => Set<MockExamSection>();
    public DbSet<MockExamQuestion> MockExamQuestions => Set<MockExamQuestion>();
    public DbSet<MockExamOption> MockExamOptions => Set<MockExamOption>();
    public DbSet<UserMockExamAttempt> UserMockExamAttempts => Set<UserMockExamAttempt>();
    public DbSet<UserMockExamAnswer> UserMockExamAnswers => Set<UserMockExamAnswer>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
    public DbSet<AssessmentOption> AssessmentOptions => Set<AssessmentOption>();
    public DbSet<UserAssessmentResult> UserAssessmentResults => Set<UserAssessmentResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
            entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.Phone).IsUnique();
        });

        modelBuilder.Entity<ProfileCharacter>(entity =>
        {
            entity.ToTable("profile_character");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(x => x.Prompt).HasColumnName("prompt").IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.ToTable("profile");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.ProfileCharacterId).HasColumnName("profile_character_id").IsRequired();
            entity.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(255);
            entity.Property(x => x.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(x => x.Bio).HasColumnName("bio");
            entity.Property(x => x.Birthday).HasColumnName("birthday");
            entity.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(20);
            entity.Property(x => x.Location).HasColumnName("location").HasMaxLength(255);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.UserId).IsUnique();

            entity.HasOne<User>()
                .WithOne()
                .HasForeignKey<Profile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ProfileCharacter>()
                .WithMany()
                .HasForeignKey(x => x.ProfileCharacterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SystemPrompt>(entity =>
        {
            entity.ToTable("system_prompt");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.NoiDungPrompt).HasColumnName("noi_dung_prompt").IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150);
            entity.Property(x => x.IsDefault).HasColumnName("is_default");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ApiSetting>(entity =>
        {
            entity.ToTable("api_setting");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.ApiKey).HasColumnName("api_key").IsRequired();
            entity.Property(x => x.Provider).HasColumnName("provider").IsRequired().HasMaxLength(100);
            entity.Property(x => x.BaseUrl).HasColumnName("base_url");
            entity.Property(x => x.ModelName).HasColumnName("model_name").HasMaxLength(100);
            entity.Property(x => x.Temperature).HasColumnName("temperature").HasPrecision(3, 2);
            entity.Property(x => x.MaxTokens).HasColumnName("max_tokens");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("product_category");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.Code).HasColumnName("code").IsRequired().HasMaxLength(50);
            entity.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.SortOrder).HasColumnName("sort_order");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("course");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.ProductCategoryId).HasColumnName("product_category_id").IsRequired();
            entity.Property(x => x.Code).HasColumnName("code").IsRequired().HasMaxLength(50);
            entity.Property(x => x.Slug).HasColumnName("slug").IsRequired().HasMaxLength(150);
            entity.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(x => x.ShortDescription).HasColumnName("short_description");
            entity.Property(x => x.ThumbnailUrl).HasColumnName("thumbnail_url");
            entity.Property(x => x.LevelCode).HasColumnName("level_code").HasMaxLength(20);
            entity.Property(x => x.DurationMonths).HasColumnName("duration_months");
            entity.Property(x => x.Price).HasColumnName("price").HasPrecision(14, 2);
            entity.Property(x => x.OriginalPrice).HasColumnName("original_price").HasPrecision(14, 2);
            entity.Property(x => x.Currency).HasColumnName("currency").IsRequired().HasMaxLength(10);
            entity.Property(x => x.IsFree).HasColumnName("is_free");
            entity.Property(x => x.IsHot).HasColumnName("is_hot");
            entity.Property(x => x.IsPublished).HasColumnName("is_published");
            entity.Property(x => x.SortOrder).HasColumnName("sort_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasOne(x => x.ProductCategory)
                .WithMany(x => x.Courses)
                .HasForeignKey(x => x.ProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("lesson");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.CourseId).HasColumnName("course_id").IsRequired();
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50);
            entity.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(x => x.LessonKind).HasColumnName("lesson_kind").IsRequired().HasMaxLength(30);
            entity.Property(x => x.ShortDescription).HasColumnName("short_description");
            entity.Property(x => x.SortOrder).HasColumnName("sort_order");
            entity.Property(x => x.IsPreview).HasColumnName("is_preview");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.Course)
                .WithMany(x => x.Lessons)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserCourse>(entity =>
        {
            entity.ToTable("user_course");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.CourseId).HasColumnName("course_id").IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(30);
            entity.Property(x => x.ProgressPercent).HasColumnName("progress_percent").HasPrecision(5, 2);
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.ExpiredAt).HasColumnName("expired_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => new { x.UserId, x.CourseId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Course)
                .WithMany(x => x.UserCourses)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserLessonProgress>(entity =>
        {
            entity.ToTable("user_lesson_progress");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.LessonId).HasColumnName("lesson_id").IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(30);
            entity.Property(x => x.ProgressPercent).HasColumnName("progress_percent").HasPrecision(5, 2);
            entity.Property(x => x.LastVideoId).HasColumnName("last_video_id");
            entity.Property(x => x.LastPositionSeconds).HasColumnName("last_position_seconds");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => new { x.UserId, x.LessonId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Lesson)
                .WithMany()
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.LastVideo)
                .WithMany()
                .HasForeignKey(x => x.LastVideoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CourseOrder>(entity =>
        {
            entity.ToTable("course_order");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.OrderCode).HasColumnName("order_code").IsRequired().HasMaxLength(50);
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.CourseId).HasColumnName("course_id").IsRequired();
            entity.Property(x => x.Provider).HasColumnName("provider").IsRequired().HasMaxLength(30);
            entity.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            entity.Property(x => x.Amount).HasColumnName("amount").HasPrecision(14, 2);
            entity.Property(x => x.Currency).HasColumnName("currency").IsRequired().HasMaxLength(10);
            entity.Property(x => x.OrderTitle).HasColumnName("order_title").HasMaxLength(255);
            entity.Property(x => x.OrderDescription).HasColumnName("order_description");
            entity.Property(x => x.VnpTxnRef).HasColumnName("vnp_txn_ref").HasMaxLength(100);
            entity.Property(x => x.VnpOrderInfo).HasColumnName("vnp_order_info");
            entity.Property(x => x.PaymentUrl).HasColumnName("payment_url");
            entity.Property(x => x.ReturnUrl).HasColumnName("return_url");
            entity.Property(x => x.IpnUrl).HasColumnName("ipn_url");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.PaidAt).HasColumnName("paid_at");
            entity.Property(x => x.ExpiredAt).HasColumnName("expired_at");
            entity.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(x => x.CreatedByIp).HasColumnName("created_by_ip").HasMaxLength(64);
            entity.Property(x => x.Note).HasColumnName("note");

            entity.HasIndex(x => x.OrderCode).IsUnique();
            entity.HasIndex(x => x.VnpTxnRef).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.CourseId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAt).IsDescending();
            entity.HasIndex(x => new { x.UserId, x.CourseId })
                .IsUnique()
                .HasFilter("status = 'pending'");

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Course)
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("payment_transaction");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(x => x.Provider).HasColumnName("provider").IsRequired().HasMaxLength(30);
            entity.Property(x => x.TransactionType).HasColumnName("transaction_type").IsRequired().HasMaxLength(30);
            entity.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            entity.Property(x => x.RequestId).HasColumnName("request_id").HasMaxLength(100);
            entity.Property(x => x.TransactionRef).HasColumnName("transaction_ref").HasMaxLength(100);
            entity.Property(x => x.ProviderTransactionNo).HasColumnName("provider_transaction_no").HasMaxLength(100);
            entity.Property(x => x.Amount).HasColumnName("amount").HasPrecision(14, 2);
            entity.Property(x => x.Currency).HasColumnName("currency").IsRequired().HasMaxLength(10);
            entity.Property(x => x.BankCode).HasColumnName("bank_code").HasMaxLength(50);
            entity.Property(x => x.BankTranNo).HasColumnName("bank_tran_no").HasMaxLength(100);
            entity.Property(x => x.CardType).HasColumnName("card_type").HasMaxLength(50);
            entity.Property(x => x.ResponseCode).HasColumnName("response_code").HasMaxLength(20);
            entity.Property(x => x.TransactionStatusCode).HasColumnName("transaction_status_code").HasMaxLength(20);
            entity.Property(x => x.PayDate).HasColumnName("pay_date").HasMaxLength(30);
            entity.Property(x => x.SecureHash).HasColumnName("secure_hash");
            entity.Property(x => x.RawQuery).HasColumnName("raw_query");
            entity.Property(x => x.RawRequest).HasColumnName("raw_request").HasColumnType("jsonb");
            entity.Property(x => x.RawResponse).HasColumnName("raw_response").HasColumnType("jsonb");
            entity.Property(x => x.RawIpn).HasColumnName("raw_ipn").HasColumnType("jsonb");
            entity.Property(x => x.RawReturn).HasColumnName("raw_return").HasColumnType("jsonb");
            entity.Property(x => x.IpnReceivedAt).HasColumnName("ipn_received_at");
            entity.Property(x => x.ReturnReceivedAt).HasColumnName("return_received_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.Note).HasColumnName("note");

            entity.HasIndex(x => x.OrderId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.TransactionRef);
            entity.HasIndex(x => x.ProviderTransactionNo);
            entity.HasIndex(x => x.CreatedAt).IsDescending();

            entity.HasOne(x => x.Order)
                .WithMany(x => x.PaymentTransactions)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessionVideo>(entity =>
        {
            entity.ToTable("lession_video");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.LessonId).HasColumnName("lesson_id").IsRequired();
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(255);
            entity.Property(x => x.VideoUrl).HasColumnName("video_url").IsRequired();
            entity.Property(x => x.VideoProvider).HasColumnName("video_provider").HasMaxLength(50);
            entity.Property(x => x.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(x => x.TranscriptText).HasColumnName("transcript_text");
            entity.Property(x => x.SubtitleUrl).HasColumnName("subtitle_url");
            entity.Property(x => x.SortOrder).HasColumnName("sort_order");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.Lesson)
                .WithMany(x => x.Videos)
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessionText>(entity =>
        {
            entity.ToTable("lession_text");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.LessonId).HasColumnName("lesson_id").IsRequired();
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(255);
            entity.Property(x => x.ContentText).HasColumnName("content_text");
            entity.Property(x => x.ContentHtml).HasColumnName("content_html");
            entity.Property(x => x.AttachmentUrl).HasColumnName("attachment_url");
            entity.Property(x => x.SortOrder).HasColumnName("sort_order");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.Lesson)
                .WithMany(x => x.Texts)
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonQuiz>(entity =>
        {
            entity.ToTable("lesson_quiz");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.LessonId).HasColumnName("lesson_id").IsRequired();
            entity.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.PassingScore).HasColumnName("passing_score").HasPrecision(5, 2);
            entity.Property(x => x.TimeLimitMinutes).HasColumnName("time_limit_minutes");
            entity.Property(x => x.MaxAttempts).HasColumnName("max_attempts");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.LessonId).IsUnique();

            entity.HasOne(x => x.Lesson)
                .WithOne(x => x.Quiz)
                .HasForeignKey<LessonQuiz>(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonQuizQuestion>(entity =>
        {
            entity.ToTable("lesson_quiz_question");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.LessonQuizId).HasColumnName("lesson_quiz_id").IsRequired();
            entity.Property(x => x.QuestionText).HasColumnName("question_text").IsRequired();
            entity.Property(x => x.ExplanationText).HasColumnName("explanation_text");
            entity.Property(x => x.SortOrder).HasColumnName("sort_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.LessonQuiz)
                .WithMany(x => x.Questions)
                .HasForeignKey(x => x.LessonQuizId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonQuizOption>(entity =>
        {
            entity.ToTable("lesson_quiz_option");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.LessonQuizQuestionId).HasColumnName("lesson_quiz_question_id").IsRequired();
            entity.Property(x => x.OptionLabel).HasColumnName("option_label").HasMaxLength(10);
            entity.Property(x => x.OptionText).HasColumnName("option_text").IsRequired();
            entity.Property(x => x.IsCorrect).HasColumnName("is_correct");
            entity.Property(x => x.SortOrder).HasColumnName("sort_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.LessonQuizQuestion)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.LessonQuizQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonDiscussion>(entity =>
        {
            entity.ToTable("lesson_discussion");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.LessonId).HasColumnName("lesson_id").IsRequired();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.ParentId).HasColumnName("parent_id");
            entity.Property(x => x.Content).HasColumnName("content").IsRequired();
            entity.Property(x => x.LikeCount).HasColumnName("like_count");
            entity.Property(x => x.ReplyCount).HasColumnName("reply_count");
            entity.Property(x => x.IsEdited).HasColumnName("is_edited");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.LessonId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.ParentId);
            entity.HasIndex(x => x.CreatedAt).IsDescending();

            entity.HasOne(x => x.Lesson)
                .WithMany()
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Parent)
                .WithMany(x => x.Replies)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonDiscussionLike>(entity =>
        {
            entity.ToTable("lesson_discussion_like");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.DiscussionId).HasColumnName("discussion_id").IsRequired();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(x => x.DiscussionId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.DiscussionId, x.UserId }).IsUnique();

            entity.HasOne(x => x.Discussion)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.DiscussionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vocabulary>(entity =>
        {
            entity.ToTable("vocabulary");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.Kanji).HasColumnName("kanji").IsRequired().HasMaxLength(255);
            entity.Property(x => x.Kana).HasColumnName("kana").HasMaxLength(255);
            entity.Property(x => x.MeaningVi).HasColumnName("meaning_vi").IsRequired().HasMaxLength(255);
            entity.Property(x => x.LevelCode).HasColumnName("level_code").HasMaxLength(20);
            entity.Property(x => x.CategoryCode).HasColumnName("category_code").HasMaxLength(50);
            entity.Property(x => x.ExampleText).HasColumnName("example_text");
            entity.Property(x => x.ExampleMeaningVi).HasColumnName("example_meaning_vi");
            entity.Property(x => x.SortOrder).HasColumnName("sort_order");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<UserFavoriteVocabulary>(entity =>
        {
            entity.ToTable("user_favorite_vocabulary");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.VocabularyId).HasColumnName("vocabulary_id").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(x => new { x.UserId, x.VocabularyId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Vocabulary)
                .WithMany(x => x.UserFavoriteVocabularies)
                .HasForeignKey(x => x.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notification");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(x => x.Content).HasColumnName("content").IsRequired();
            entity.Property(x => x.Type).HasColumnName("type").IsRequired().HasMaxLength(50);
            entity.Property(x => x.TargetUrl).HasColumnName("target_url").HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.ToTable("user_notification");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.NotificationId).HasColumnName("notification_id").IsRequired();
            entity.Property(x => x.IsRead).HasColumnName("is_read");
            entity.Property(x => x.ReadAt).HasColumnName("read_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(x => new { x.UserId, x.NotificationId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Notification)
                .WithMany(x => x.UserNotifications)
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MockExam>(entity =>
        {
            entity.ToTable("mock_exam");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(x => x.Level).HasColumnName("level").IsRequired().HasMaxLength(10);
            entity.Property(x => x.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(x => x.PassingScore).HasColumnName("passing_score");
            entity.Property(x => x.TotalScore).HasColumnName("total_score");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.IsPublished).HasColumnName("is_published");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<MockExamSection>(entity =>
        {
            entity.ToTable("mock_exam_section");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.MockExamId).HasColumnName("mock_exam_id").IsRequired();
            entity.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(x => x.OrderIndex).HasColumnName("order_index");
            entity.Property(x => x.TimeLimitMinutes).HasColumnName("time_limit_minutes");

            entity.HasOne(x => x.MockExam)
                .WithMany(x => x.Sections)
                .HasForeignKey(x => x.MockExamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MockExamQuestion>(entity =>
        {
            entity.ToTable("mock_exam_question");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.SectionId).HasColumnName("section_id").IsRequired();
            entity.Property(x => x.QuestionText).HasColumnName("question_text").IsRequired();
            entity.Property(x => x.AudioUrl).HasColumnName("audio_url").HasMaxLength(500);
            entity.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
            entity.Property(x => x.Explanation).HasColumnName("explanation");
            entity.Property(x => x.Points).HasColumnName("points");
            entity.Property(x => x.OrderIndex).HasColumnName("order_index");

            entity.HasOne(x => x.Section)
                .WithMany(x => x.Questions)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MockExamOption>(entity =>
        {
            entity.ToTable("mock_exam_option");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.QuestionId).HasColumnName("question_id").IsRequired();
            entity.Property(x => x.OptionText).HasColumnName("option_text").IsRequired();
            entity.Property(x => x.IsCorrect).HasColumnName("is_correct");
            entity.Property(x => x.OrderIndex).HasColumnName("order_index");

            entity.HasOne(x => x.Question)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserMockExamAttempt>(entity =>
        {
            entity.ToTable("user_mock_exam_attempt");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.MockExamId).HasColumnName("mock_exam_id").IsRequired();
            entity.Property(x => x.Score).HasColumnName("score");
            entity.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            entity.Property(x => x.IsPassed).HasColumnName("is_passed");
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.SubmittedAt).HasColumnName("submitted_at");

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.MockExam)
                .WithMany(x => x.Attempts)
                .HasForeignKey(x => x.MockExamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserMockExamAnswer>(entity =>
        {
            entity.ToTable("user_mock_exam_answer");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.AttemptId).HasColumnName("attempt_id").IsRequired();
            entity.Property(x => x.QuestionId).HasColumnName("question_id").IsRequired();
            entity.Property(x => x.SelectedOptionId).HasColumnName("selected_option_id");
            entity.Property(x => x.IsCorrect).HasColumnName("is_correct");
            entity.Property(x => x.PointsAwarded).HasColumnName("points_awarded");

            entity.HasOne(x => x.Attempt)
                .WithMany(x => x.Answers)
                .HasForeignKey(x => x.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Question)
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.SelectedOption)
                .WithMany()
                .HasForeignKey(x => x.SelectedOptionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AssessmentQuestion>(entity =>
        {
            entity.ToTable("assessment_question");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.QuestionText).HasColumnName("question_text").IsRequired();
            entity.Property(x => x.Level).HasColumnName("level").IsRequired().HasMaxLength(10);
            entity.Property(x => x.Explanation).HasColumnName("explanation");
            entity.Property(x => x.OrderIndex).HasColumnName("order_index");
        });

        modelBuilder.Entity<AssessmentOption>(entity =>
        {
            entity.ToTable("assessment_option");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.QuestionId).HasColumnName("question_id").IsRequired();
            entity.Property(x => x.OptionText).HasColumnName("option_text").IsRequired();
            entity.Property(x => x.IsCorrect).HasColumnName("is_correct");

            entity.HasOne(x => x.Question)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserAssessmentResult>(entity =>
        {
            entity.ToTable("user_assessment_result");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.RecommendedLevel).HasColumnName("recommended_level").IsRequired().HasMaxLength(10);
            entity.Property(x => x.TotalScore).HasColumnName("total_score");
            entity.Property(x => x.MaxScore).HasColumnName("max_score");
            entity.Property(x => x.TakenAt).HasColumnName("taken_at");

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
