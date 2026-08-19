using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Entities;

namespace Persistence.Configuration
{
    public sealed class CharacterConfiguration : IEntityTypeConfiguration<CharacterModel>
    {
        public void Configure(EntityTypeBuilder<CharacterModel> builder)
        {
            builder.HasIndex(character => character.UserId)
                .IsUnique();

            builder.HasOne(character => character.User)
                .WithOne()
                .HasForeignKey<CharacterModel>(character => character.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
