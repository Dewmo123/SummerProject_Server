using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SummerLoginServer.Entities
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Username { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
    public class ExternalLogin
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public LoginProvider Provider { get; set; }
        public string ProviderUserId { get; set; } = null!;
    }
    public enum LoginProvider
    {
        Google = 1,
        Facebook = 2,
        Guest = 999
    }
}
