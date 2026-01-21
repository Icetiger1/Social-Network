using Domain.Abstractions;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// Конфигурация для TopicId
/// </summary>
public class TopicIdConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        // Первичный ключ
        builder.HasKey(e => e.Id);

        // Конвертер для TopicId
        builder.Property(topic => topic.Id)
            .HasConversion(
                id => id.Value,
                value => TopicId.Of(value)
            );

        // Конфигурация свойств
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Summary)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.TopicType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.EventStart)
            .IsRequired(false);

        // Owned type для Location
        builder.OwnsOne(t => t.Location, location =>
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

        // Теневые свойства для аудита (если не в доменной модели)
        builder.Property<DateTime>("CreatedAt")
            .IsRequired();

        builder.Property<DateTime?>("UpdatedAt");

        builder.Property<DateTime?>("DeletedAt");

        // Индексы
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.DeletedAt);

        // Фильтр для мягкого удаления
        builder.HasQueryFilter(t => t.DeletedAt == null);
    }
}