using System;
using System.Collections.Generic;
using System.Text;

namespace Persistence.Entities
{
    public class Character
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int Level { get; set; } = 1;
        public long Exp { get; set; } = 0;
    }
}
