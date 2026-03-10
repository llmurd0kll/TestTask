using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Data
    {
    public class AppDbContext : DbContext
        {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
            {
            }

        // Таблица с короткими ссылками
        public DbSet<ShortUrl> ShortUrls { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            base.OnModelCreating(modelBuilder);

            // Индекс по ShortCode, чтобы быстро искать по короткому коду
            modelBuilder.Entity<ShortUrl>()
                .HasIndex(x => x.ShortCode)
                .IsUnique();
            }
        }
    }
