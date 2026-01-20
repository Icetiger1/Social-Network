global using Application.Data.DataBaseContext;
global using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Data.DataBaseContext;

/// <summary>
/// Application Database Context
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Topic> Topics => Set<Topic>();

    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly()
        );
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    private static void ConfigureTopic(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Topic>(entity =>
        {
            // Первичный ключ
            entity.HasKey(e => e.Id);

            // Конвертер для TopicId
            entity.Property(e => e.Id)
                .HasConversion(
                    id => id.Value,
                    value => TopicId.Of(value))
                .HasColumnName("Id");

            // Конфигурация свойств
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Summary)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.TopicType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.EventStart)
                .IsRequired(false);

            // Owned type для Location
            entity.OwnsOne(t => t.Location, location =>
            {
                location.Property(l => l.City)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("City");

                location.Property(l => l.Street)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("Street");
            });

            // Теньвые свойства для аудита (если не в доменной модели)
            entity.Property<DateTime>("CreatedAt")
                .IsRequired();

            entity.Property<DateTime?>("UpdatedAt");

            entity.Property<DateTime?>("DeletedAt");

            // Индексы
            entity.HasIndex(t => t.CreatedAt);
            entity.HasIndex(t => t.IsDeleted);

            // Фильтр для мягкого удаления
            entity.HasQueryFilter(t => !t.IsDeleted);
        });
    }
}
