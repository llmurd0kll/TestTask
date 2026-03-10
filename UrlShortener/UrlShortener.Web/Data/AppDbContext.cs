using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Data
    {
    public class AppDbContext : DbContext
        {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
            {
            // Почему: DbContextOptions передаются через DI, чтобы можно было
            // легко менять провайдер БД (MySQL → InMemory → SQLite) без изменения кода.
            }

        // Почему: отдельная таблица для коротких ссылок.
        // EF Core сам создаёт таблицу на основе модели ShortUrl.
        public DbSet<ShortUrl> ShortUrls { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            base.OnModelCreating(modelBuilder);

            // Почему: поиск по короткому коду — самая частая операция в приложении.
            // Индекс ускоряет SELECT r/{code} в десятки раз.
            // Уникальность гарантирует, что два разных URL не получат одинаковый код.
            modelBuilder.Entity<ShortUrl>()
                .HasIndex(x => x.ShortCode)
                .IsUnique();
            }
        }
    }
