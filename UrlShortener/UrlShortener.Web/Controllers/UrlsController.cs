using Microsoft.AspNetCore.Mvc;
using UrlShortener.Web.Services;

namespace UrlShortener.Web.Controllers
    {
    public class UrlsController : Controller
        {
        private readonly UrlService _service;

        public UrlsController(UrlService service)
            {
            _service = service;
            }

        // GET: /Urls
        public async Task<IActionResult> Index()
            {
            var items = await _service.GetAllAsync();
            return View(items);
            }

        // POST: /Urls/Create
        [HttpPost]
        public async Task<IActionResult> Create(string longUrl)
            {
            if (string.IsNullOrWhiteSpace(longUrl))
                {
                TempData["Error"] = "Введите URL";
                return RedirectToAction("Index");
                }

            try
                {
                await _service.CreateAsync(longUrl);
                }
            catch (Exception ex)
                {
                TempData["Error"] = ex.Message;
                }

            return RedirectToAction("Index");
            }

        // GET: /Urls/Edit/5
        public async Task<IActionResult> Edit(int id)
            {
            var items = await _service.GetAllAsync();
            var item = items.FirstOrDefault(x => x.Id == id);

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
                await _service.UpdateAsync(id, longUrl);
                }
            catch (Exception ex)
                {
                TempData["Error"] = ex.Message;
                }

            return RedirectToAction("Index");
            }

        // POST: /Urls/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
            {
            await _service.DeleteAsync(id);
            return RedirectToAction("Index");
            }

        // GET: /{shortCode}
        [Route("r/{code}")]
        public async Task<IActionResult> RedirectToLong(string code)
            {
            var item = await _service.GetByCodeAndIncrementAsync(code);

            if (item == null)
                return NotFound();

            // Кодируем URL в ASCII (один раз)
            var encoded = Uri.EscapeUriString(item.LongUrl);

            // Возвращаем то, что Kestrel может положить в заголовок
            return Redirect(encoded);
            }
        }
    }
