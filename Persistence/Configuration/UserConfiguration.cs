using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Entities;

namespace Persistence.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(u => u.Username)
                .IsUnique();

            builder.HasIndex(u => new
            {
                u.Provider,
                u.ProviderUserId
            }).IsUnique();

            builder.Property(u => u.ProviderUserId)
                .HasMaxLength(255);
        }
    }
}
