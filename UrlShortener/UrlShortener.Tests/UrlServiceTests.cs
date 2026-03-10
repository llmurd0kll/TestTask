using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Data;
using UrlShortener.Web.Services;
using Xunit;

public class UrlServiceTests
    {
    private AppDbContext CreateDb()
        {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
        }

    [Fact]
    public async Task CreateAsync_ShouldCreateShortUrl()
        {
        var db = CreateDb();
        var service = new UrlService(db);

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
        var service = new UrlService(db);

        var a = await service.CreateAsync("https://a.com");
        var b = await service.CreateAsync("https://b.com");

        Assert.NotEqual(a.ShortCode, b.ShortCode);
        }

    [Fact]
    public async Task GetByCodeAndIncrementAsync_ShouldIncreaseClicks()
        {
        var db = CreateDb();
        var service = new UrlService(db);

        var created = await service.CreateAsync("https://example.com");

        var before = created.Clicks;

        var loaded = await service.GetByCodeAndIncrementAsync(created.ShortCode);

        Assert.Equal(before + 1, loaded.Clicks);
        }

    [Fact]
    public async Task CreateAsync_ShouldThrow_OnInvalidUrl()
        {
        var db = CreateDb();
        var service = new UrlService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync("not-a-url"));
        }
    }
