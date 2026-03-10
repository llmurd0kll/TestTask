using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UrlShortener.Web.Data;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Services
    {
    public class UrlService
        {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        public UrlService(AppDbContext db, IMemoryCache cache)
            {
            // Почему: сервис получает зависимости через DI, чтобы быть тестируемым
            // и не зависеть от конкретной реализации БД или кэша.
            _db = db;
            _cache = cache;
            }

        /// <summary>
        /// Создаёт новую короткую ссылку.
        /// Почему: сервис отвечает за бизнес‑логику — валидацию, генерацию кода,
        /// проверку уникальности и сохранение в БД.
        /// </summary>
        public async Task<ShortUrl> CreateAsync(string longUrl)
            {
            // Почему: валидация на уровне сервиса защищает БД от мусорных данных.
            if (!Uri.IsWellFormedUriString(longUrl, UriKind.Absolute))
                throw new ArgumentException("Некорректный URL");

            string code;

            // Почему: даже криптостойкий генератор может дать коллизию,
            // поэтому проверяем уникальность в цикле.
            do
                {
                code = CodeGenerator.GenerateShortCode();
                }
            while (await _db.ShortUrls.AnyAsync(x => x.ShortCode == code));

            var entity = new ShortUrl
                {
                LongUrl = longUrl,
                ShortCode = code,
                CreatedAt = DateTime.UtcNow, // Почему: UTC исключает проблемы с часовыми поясами.
                Clicks = 0
                };

            // Почему: Add достаточно — EF сам отслеживает сущность.
            _db.ShortUrls.Add(entity);
            await _db.SaveChangesAsync(); // Почему: сохраняем сразу, чтобы код стал доступен.

            return entity;
            }

        /// <summary>
        /// Возвращает ссылку и увеличивает счётчик переходов.
        /// Почему: метод нужен для совместимости, но логика вынесена в отдельные методы.
        /// </summary>
        public async Task<ShortUrl?> GetByCodeAndIncrementAsync(string code)
            {
            var item = await GetByCodeAsync(code);
            if (item == null)
                return null;

            await IncrementClicksAsync(code);
            item.Clicks += 1;
            return item;
            }

        /// <summary>
        /// Получить все записи.
        /// Почему: сортировка по CreatedAt делает вывод удобным — новые ссылки сверху.
        /// </summary>
        public async Task<List<ShortUrl>> GetAllAsync()
            {
            return await _db.ShortUrls
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            }

        /// <summary>
        /// Обновить длинный URL.
        /// Почему: сервис отвечает за валидацию и изменение данных.
        /// </summary>
        public async Task UpdateAsync(int id, string newUrl)
            {
            if (!Uri.IsWellFormedUriString(newUrl, UriKind.Absolute))
                throw new ArgumentException("Некорректный URL");

            var item = await _db.ShortUrls.FindAsync(id);

            // Почему: FindAsync быстрее, так как ищет по первичному ключу.
            if (item == null)
                throw new Exception("Запись не найдена");

            item.LongUrl = newUrl;

            await _db.SaveChangesAsync(); // Почему: изменения должны быть видны сразу.
            }

        /// <summary>
        /// Удалить запись.
        /// Почему: проверяем существование, чтобы не бросать исключения без необходимости.
        /// </summary>
        public async Task DeleteAsync(int id)
            {
            var item = await _db.ShortUrls.FindAsync(id);

            if (item == null)
                return;

            _db.ShortUrls.Remove(item);
            await _db.SaveChangesAsync();
            }

        /// <summary>
        /// Получить ссылку по короткому коду.
        /// Почему: это горячий путь (редиректы), поэтому используем кэш,
        /// чтобы снизить нагрузку на БД в десятки раз.
        /// </summary>
        public async Task<ShortUrl?> GetByCodeAsync(string code)
            {
            return await _cache.GetOrCreateAsync($"short:{code}", async entry =>
            {
                // Почему: TTL 1 минута — оптимальный баланс между свежестью и производительностью.
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                return await _db.ShortUrls
                    .AsNoTracking() // Почему: чтение, трекинг не нужен.
                    .FirstOrDefaultAsync(x => x.ShortCode == code);
            });
            }

        /// <summary>
        /// Увеличивает счётчик переходов.
        /// Почему: используем прямой SQL, чтобы избежать накладных расходов EF
        /// и ускорить операцию под высокой нагрузкой.
        /// </summary>
        public async Task IncrementClicksAsync(string code)
            {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE ShortUrls SET Clicks = Clicks + 1 WHERE ShortCode = {code}");

            // Почему: данные изменились — кэш нужно сбросить,
            // иначе пользователь увидит устаревшее значение.
            _cache.Remove($"short:{code}");
            }
        }
    }
