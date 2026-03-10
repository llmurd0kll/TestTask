using Microsoft.AspNetCore.Mvc;
using UrlShortener.Web.Services;

namespace UrlShortener.Web.Controllers
    {
    public class UrlsController : Controller
        {
        private readonly UrlService _service;

        public UrlsController(UrlService service)
            {
            // Почему: контроллер получает сервис через DI, чтобы оставаться тонким
            // и не зависеть от деталей реализации бизнес‑логики.
            _service = service;
            }

        // GET: /Urls
        public async Task<IActionResult> Index()
            {
            // Почему: контроллер не должен заниматься бизнес‑логикой.
            // Он просто запрашивает данные у сервиса и передаёт их в представление.
            var items = await _service.GetAllAsync();
            return View(items);
            }

        // POST: /Urls/Create
        [HttpPost]
        public async Task<IActionResult> Create(string longUrl)
            {
            // Почему: проверяем пустой ввод здесь, чтобы не нагружать сервис
            // и сразу дать пользователю понятную ошибку.
            if (string.IsNullOrWhiteSpace(longUrl))
                {
                TempData["Error"] = "Введите URL";
                return RedirectToAction("Index");
                }

            try
                {
                // Почему: вся логика валидации и генерации кода находится в сервисе.
                await _service.CreateAsync(longUrl);
                }
            catch (Exception ex)
                {
                // Почему: ошибки должны быть показаны пользователю,
                // но не должны приводить к падению приложения.
                TempData["Error"] = ex.Message;
                }

            return RedirectToAction("Index");
            }

        // GET: /Urls/Edit/5
        public async Task<IActionResult> Edit(int id)
            {
            // Почему: сервис возвращает список, а не одну запись.
            // Это упрощает сервис, но требует фильтрации здесь.
            var items = await _service.GetAllAsync();
            var item = items.FirstOrDefault(x => x.Id == id);

            // Почему: если запись не найдена — возвращаем 404,
            // это стандарт поведения MVC.
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
                // Почему: ошибки должны быть видны пользователю,
                // но не должны прерывать работу сайта.
                TempData["Error"] = ex.Message;
                }

            return RedirectToAction("Index");
            }

        // POST: /Urls/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
            {
            // Почему: контроллер не должен знать, как именно удаляется запись.
            await _service.DeleteAsync(id);
            return RedirectToAction("Index");
            }

        // GET: /r/{code}
        [Route("r/{code}")]
        public async Task<IActionResult> RedirectToLong(string code)
            {
            // Почему: сначала читаем ссылку — это горячий путь,
            // и здесь может работать кэш.
            var item = await _service.GetByCodeAsync(code);

            if (item == null)
                return NotFound();

            // Почему: инкремент вынесен отдельно — это позволяет
            // легко заменить его на батчинг или очередь.
            await _service.IncrementClicksAsync(code);

            // Почему: EscapeUriString защищает от некорректных символов в URL.
            var encoded = Uri.EscapeUriString(item.LongUrl);
            return Redirect(encoded);
            }
        }
    }
