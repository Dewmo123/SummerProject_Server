using Persistence.Entities;
using System.ComponentModel.DataAnnotations;

namespace Persistence.Entities
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Username { get; set; } = null!;
        public LoginProvider Provider { get; set; }
        public string ProviderUserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Character? Character { get; set; }

    }
    public enum LoginProvider
    {
        Google = 1,
        Facebook = 2,
        Guest = 999
    }
}
