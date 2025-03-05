using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TimeCapsule.Models.DatabaseModels;
using Microsoft.AspNetCore.Identity;

namespace TimeCapsule.Models
{
    public class TimeCapsuleContext : IdentityDbContext<IdentityUser>
    {
        public TimeCapsuleContext(DbContextOptions options) : base(options) { }

        public DbSet<Capsule> Capsules { get; set; }
        public DbSet<CapsuleAttachment> CapsuleAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
