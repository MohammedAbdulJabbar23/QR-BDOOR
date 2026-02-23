using AlBadour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlBadour.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        builder.Property(n => n.RecipientUserId).HasColumnName("recipient_user_id");
        builder.Property(n => n.TitleAr).HasColumnName("title_ar").HasMaxLength(255).IsRequired();
        builder.Property(n => n.TitleEn).HasColumnName("title_en").HasMaxLength(255).IsRequired();
        builder.Property(n => n.MessageAr).HasColumnName("message_ar").IsRequired();
        builder.Property(n => n.MessageEn).HasColumnName("message_en").IsRequired();
        builder.Property(n => n.EntityType).HasColumnName("entity_type").HasMaxLength(50);
        builder.Property(n => n.EntityId).HasColumnName("entity_id").HasMaxLength(50);
        builder.Property(n => n.IsRead).HasColumnName("is_read").HasDefaultValue(false);
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

        builder.HasOne(n => n.Recipient).WithMany(u => u.Notifications).HasForeignKey(n => n.RecipientUserId);

        builder.HasIndex(n => new { n.RecipientUserId, n.IsRead, n.CreatedAt }).IsDescending(false, false, true);
    }
}
