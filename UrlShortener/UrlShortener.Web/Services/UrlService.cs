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
            // Почему: сервис работает с БД, но не создаёт её сам.
            // DbContext передаётся через DI, чтобы можно было легко менять провайдер
            // (MySQL → InMemory → SQLite) и тестировать сервис изолированно.
            _db = db;
            }

        /// <summary>
        /// Создаёт новую короткую ссылку.
        /// Почему: сервис отвечает за бизнес‑логику — валидацию, генерацию кода,
        /// проверку уникальности и сохранение в БД.
        /// </summary>
        public async Task<ShortUrl> CreateAsync(string longUrl)
            {
            // Почему: проверяем URL здесь, чтобы не сохранять в БД мусор.
            // Это защита от ошибок пользователя и потенциальных XSS/инъекций.
            if (!Uri.IsWellFormedUriString(longUrl, UriKind.Absolute))
                throw new ArgumentException("Некорректный URL");

            string code;

            // Почему: короткий код должен быть уникальным.
            // Даже при криптостойкой генерации возможны коллизии,
            // поэтому проверяем в цикле, пока не найдём свободный код.
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

            // Почему: AddAsync не нужен — Add достаточно, EF сам отслеживает сущность.
            _db.ShortUrls.Add(entity);
            await _db.SaveChangesAsync(); // Почему: сохраняем сразу, чтобы код стал доступен.

            return entity;
            }

        /// <summary>
        /// Возвращает ссылку по короткому коду и увеличивает счётчик переходов.
        /// Почему: ТЗ требует считать переходы, и это логично делать в одном методе,
        /// чтобы избежать гонок и двойных запросов.
        /// </summary>
        public async Task<ShortUrl?> GetByCodeAndIncrementAsync(string code)
            {
            // Почему: FirstOrDefaultAsync быстрее, чем SingleOrDefaultAsync,
            // и не бросает исключение при дубликатах (которых быть не должно).
            var item = await _db.ShortUrls.FirstOrDefaultAsync(x => x.ShortCode == code);

            if (item == null)
                return null;

            // Почему: инкрементируем здесь, а не в контроллере —
            // бизнес‑логика должна быть в сервисе.
            item.Clicks++;

            await _db.SaveChangesAsync(); // Почему: фиксируем переход сразу.

            return item;
            }

        /// <summary>
        /// Получить все записи для таблицы.
        /// Почему: сортировка по CreatedAt делает вывод более удобным —
        /// новые ссылки сверху.
        /// </summary>
        public async Task<List<ShortUrl>> GetAllAsync()
            {
            return await _db.ShortUrls
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            }

        /// <summary>
        /// Обновить длинный URL.
        /// Почему: редактирование — часть требований ТЗ.
        /// </summary>
        public async Task UpdateAsync(int id, string newUrl)
            {
            // Почему: повторяем валидацию — нельзя доверять данным из формы.
            if (!Uri.IsWellFormedUriString(newUrl, UriKind.Absolute))
                throw new ArgumentException("Некорректный URL");

            var item = await _db.ShortUrls.FindAsync(id);

            // Почему: FindAsync быстрее, чем FirstOrDefaultAsync,
            // так как ищет по первичному ключу.
            if (item == null)
                throw new Exception("Запись не найдена");

            item.LongUrl = newUrl;

            await _db.SaveChangesAsync(); // Почему: сохраняем сразу, чтобы изменения были видны.
            }

        /// <summary>
        /// Удалить запись.
        /// Почему: удаление — простая операция, но важно проверять существование.
        /// </summary>
        public async Task DeleteAsync(int id)
            {
            var item = await _db.ShortUrls.FindAsync(id);

            // Почему: если записи нет — просто выходим.
            // Это упрощает логику контроллера.
            if (item == null)
                return;

            _db.ShortUrls.Remove(item);
            await _db.SaveChangesAsync();
            }
        }
    }
