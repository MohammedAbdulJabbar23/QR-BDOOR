using AlBadour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlBadour.Infrastructure.Persistence.Configurations;

public class DocumentNumberSequenceConfiguration : IEntityTypeConfiguration<DocumentNumberSequence>
{
    public void Configure(EntityTypeBuilder<DocumentNumberSequence> builder)
    {
        builder.ToTable("document_number_sequences");

        builder.HasKey(s => s.Year);
        builder.Property(s => s.Year).HasColumnName("year");
        builder.Property(s => s.LastNumber).HasColumnName("last_number").HasDefaultValue(0);
    }
}
