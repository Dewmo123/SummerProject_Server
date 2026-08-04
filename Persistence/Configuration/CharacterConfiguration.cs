using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Entities;

namespace Persistence.Configuration
{
    public sealed class CharacterConfiguration : IEntityTypeConfiguration<Character>
    {
        public void Configure(EntityTypeBuilder<Character> builder)
        {
            builder.HasIndex(character => character.UserId)
                .IsUnique();

            builder.HasOne(character => character.User)
                .WithOne()
                .HasForeignKey<Character>(character => character.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
