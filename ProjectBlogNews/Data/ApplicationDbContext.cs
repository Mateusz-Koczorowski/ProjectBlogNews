using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectBlogNews.Data;
using ProjectBlogNews.Models;

namespace ProjectBlogNews.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .Property(e => e.FirstName)
                .IsRequired(true)
                .HasMaxLength(50);

            builder.Entity<ApplicationUser>()
                .Property(e => e.LastName)
                .IsRequired(true)
                .HasMaxLength(64);

            builder.Entity<ApplicationUser>()
                .Property(e => e.BirthDate)
                .IsRequired(true);
        }

        public DbSet<ProjectBlogNews.Models.Article>? Article { get; set; }

        public DbSet<ProjectBlogNews.Models.Subscription>? Subscription { get; set; }
    }
}
