using Domain.Abstractions;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// Конфигурация для TopicId
/// </summary>
public class TopicIdConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(topic => topic.Id)
            .HasConversion(
                id => id.Value,
                value => TopicId.Of(value)
            );

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


        builder.Property<DateTimeOffset>("CreatedAt")
            .IsRequired();
        builder.Property<DateTimeOffset?>("UpdatedAt");
        builder.Property<DateTimeOffset?>("DeletedAt");

        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.DeletedAt);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}