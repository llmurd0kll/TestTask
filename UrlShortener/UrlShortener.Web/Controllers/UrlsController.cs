using Microsoft.AspNetCore.Mvc;
using UrlShortener.Web.Services;

namespace UrlShortener.Web.Controllers
    {
    public class UrlsController : Controller
        {
        private readonly UrlService _service;

        public UrlsController(UrlService service)
            {
            // Почему: контроллер не должен знать, как работает бизнес‑логика.
            // Он получает готовый сервис через DI, чтобы оставаться тонким и тестируемым.
            _service = service;
            }

        // GET: /Urls
        public async Task<IActionResult> Index()
            {
            // Почему: список ссылок нужен для отображения таблицы на главной странице.
            // Контроллер не фильтрует и не сортирует — это ответственность сервиса.
            var items = await _service.GetAllAsync();
            return View(items);
            }

        // POST: /Urls/Create
        [HttpPost]
        public async Task<IActionResult> Create(string longUrl)
            {
            // Почему: пустой URL — частая ошибка пользователя.
            // Проверяем здесь, чтобы не нагружать сервис лишними вызовами.
            if (string.IsNullOrWhiteSpace(longUrl))
                {
                TempData["Error"] = "Введите URL";
                return RedirectToAction("Index");
                }

            try
                {
                // Почему: вся логика проверки и генерации кода находится в сервисе.
                // Контроллер только вызывает операцию.
                await _service.CreateAsync(longUrl);
                }
            catch (Exception ex)
                {
                // Почему: ошибки должны отображаться пользователю, но не ломать приложение.
                TempData["Error"] = ex.Message;
                }

            return RedirectToAction("Index");
            }

        // GET: /Urls/Edit/5
        public async Task<IActionResult> Edit(int id)
            {
            // Почему: сервис возвращает все ссылки, а не одну.
            // Это упрощает сервис, но требует фильтрации здесь.
            var items = await _service.GetAllAsync();
            var item = items.FirstOrDefault(x => x.Id == id);

            // Почему: если ссылка не найдена — возвращаем 404.
            // Это стандарт поведения MVC.
            if (item == null)
                return NotFound();

            return View(item);
            }

        // POST: /Urls/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string longUrl)
            {
            try
                {
                // Почему: обновление URL — бизнес‑логика, поэтому делегируем сервису.
                await _service.UpdateAsync(id, longUrl);
                }
            catch (Exception ex)
                {
                // Почему: ошибки должны быть видны пользователю, но не прерывать работу сайта.
                TempData["Error"] = ex.Message;
                }

            return RedirectToAction("Index");
            }

        // POST: /Urls/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
            {
            // Почему: удаление — простая операция, контроллер не должен знать деталей.
            await _service.DeleteAsync(id);
            return RedirectToAction("Index");
            }

        // GET: /r/{code}
        [Route("r/{code}")]
        public async Task<IActionResult> RedirectToLong(string code)
            {
            // Почему: при переходе нужно увеличить счётчик кликов.
            // Это делается в сервисе, чтобы контроллер оставался тонким.
            var item = await _service.GetByCodeAndIncrementAsync(code);

            // Почему: если код не найден — возвращаем 404, как требует HTTP‑спецификация.
            if (item == null)
                return NotFound();

            // Почему: Kestrel не принимает не‑ASCII символы в заголовке Location.
            // Поэтому URL нужно закодировать заранее, иначе сервер упадёт.
            var encoded = Uri.EscapeUriString(item.LongUrl);

            // Почему: RedirectResult сам формирует заголовок Location.
            // Мы передаём уже безопасный ASCII‑URL.
            return Redirect(encoded);
            }
        }
    }
