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
    }
}
