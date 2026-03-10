using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Data;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Services
    {
    public class UrlService
        {
        private readonly AppDbContext _db;

        public UrlService(AppDbContext db)
            {
            _db = db;
            }

        /// <summary>
        /// Создаёт новую короткую ссылку.
        /// Почему: проверяем URL, генерируем уникальный код, сохраняем в БД.
        /// </summary>
        public async Task<ShortUrl> CreateAsync(string longUrl)
            {

            if (!Uri.IsWellFormedUriString(longUrl, UriKind.Absolute))
                throw new ArgumentException("Некорректный URL");

            string code;

            // Генерируем код, пока не найдём уникальный
            do
                {
                code = CodeGenerator.GenerateShortCode();
                }
            while (await _db.ShortUrls.AnyAsync(x => x.ShortCode == code));

            var entity = new ShortUrl
                {
                LongUrl = longUrl,
                ShortCode = code,
                CreatedAt = DateTime.UtcNow,
                Clicks = 0
                };

            _db.ShortUrls.Add(entity);
            await _db.SaveChangesAsync();

            return entity;
            }

        /// <summary>
        /// Увеличивает счётчик переходов.
        /// Почему: ТЗ требует считать переходы.
        /// </summary>
        public async Task<ShortUrl?> GetByCodeAndIncrementAsync(string code)
            {
            Console.WriteLine(">>> Searching for code = [" + code + "]");
            var item = await _db.ShortUrls.FirstOrDefaultAsync(x => x.ShortCode == code);

            if (item == null)
                {
                Console.WriteLine(">>> NOT FOUND");
                return null;
                }
            item.Clicks++;
            await _db.SaveChangesAsync();

            return item;
            }

        /// <summary>
        /// Получить все записи для таблицы.
        /// </summary>
        public async Task<List<ShortUrl>> GetAllAsync()
            {
            return await _db.ShortUrls
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            }

        /// <summary>
        /// Обновить длинный URL.
        /// </summary>
        public async Task UpdateAsync(int id, string newUrl)
            {

            if (!Uri.IsWellFormedUriString(newUrl, UriKind.Absolute))
                throw new ArgumentException("Некорректный URL");

            var item = await _db.ShortUrls.FindAsync(id);
            if (item == null)
                throw new Exception("Запись не найдена");

            item.LongUrl = newUrl;
            await _db.SaveChangesAsync();
            }

        /// <summary>
        /// Удалить запись.
        /// </summary>
        public async Task DeleteAsync(int id)
            {
            var item = await _db.ShortUrls.FindAsync(id);
            if (item == null)
                return;

            _db.ShortUrls.Remove(item);
            await _db.SaveChangesAsync();
            }

        }
    }
