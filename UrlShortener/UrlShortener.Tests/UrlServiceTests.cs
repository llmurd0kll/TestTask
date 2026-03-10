using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UrlShortener.Web.Data;
using UrlShortener.Web.Services;

public class UrlServiceTests
    {
    private AppDbContext CreateDb()
        {
        // Почему: EF InMemory не поддерживает SQL-команды (ExecuteSqlInterpolatedAsync),
        // которые мы используем для оптимизации инкремента кликов.
        // Поэтому для тестов используем SQLite InMemory — это настоящая реляционная БД,
        // которая работает в памяти и поддерживает SQL.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var db = new AppDbContext(options);

        // Почему: SQLite InMemory создаёт БД только пока открыто соединение.
        // Если не открыть вручную — база будет пересоздаваться при каждом запросе.
        db.Database.OpenConnection();

        // Почему: EnsureCreated создаёт таблицы без миграций.
        // Это быстро и идеально подходит для юнит-тестов.
        db.Database.EnsureCreated();

        return db;
        }

    [Fact]
    public async Task CreateAsync_ShouldCreateShortUrl()
        {
        var db = CreateDb();
        var service = new UrlService(db, new MemoryCache(new MemoryCacheOptions()));

        // Почему: проверяем базовый сценарий — корректный URL должен создаваться.
        var result = await service.CreateAsync("https://example.com");

        Assert.NotNull(result);
        Assert.Equal("https://example.com", result.LongUrl);
        Assert.False(string.IsNullOrWhiteSpace(result.ShortCode));
        Assert.Equal(0, result.Clicks);
        }

    [Fact]
    public async Task CreateAsync_ShouldGenerateUniqueCodes()
        {
        var db = CreateDb();
        var service = new UrlService(db, new MemoryCache(new MemoryCacheOptions()));

        // Почему: генератор должен выдавать уникальные коды,
        // иначе сервис будет работать нестабильно под нагрузкой.
        var a = await service.CreateAsync("https://a.com");
        var b = await service.CreateAsync("https://b.com");

        Assert.NotEqual(a.ShortCode, b.ShortCode);
        }

    [Fact]
    public async Task GetByCodeAndIncrementAsync_ShouldIncreaseClicks()
        {
        var db = CreateDb();
        var service = new UrlService(db, new MemoryCache(new MemoryCacheOptions()));

        var created = await service.CreateAsync("https://example.com");

        var before = created.Clicks;

        // Почему: метод должен не только вернуть ссылку,
        // но и увеличить счётчик кликов.
        var loaded = await service.GetByCodeAndIncrementAsync(created.ShortCode);

        Assert.Equal(before + 1, loaded.Clicks);
        }

    [Fact]
    public async Task CreateAsync_ShouldThrow_OnInvalidUrl()
        {
        var db = CreateDb();
        var service = new UrlService(db, new MemoryCache(new MemoryCacheOptions()));

        // Почему: сервис обязан валидировать URL,
        // иначе в БД попадут некорректные данные.
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync("not-a-url"));
        }
    }
