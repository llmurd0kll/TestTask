using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Data;
using UrlShortener.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Почему: MVC нужен для работы контроллеров, маршрутов и Razor‑представлений.
builder.Services.AddControllersWithViews();

// Почему: кэш снижает нагрузку на БД при частых редиректах (горячий путь).
builder.Services.AddMemoryCache();

// Почему: UrlService содержит бизнес‑логику, и его жизненный цикл должен совпадать
// с жизненным циклом HTTP‑запроса.
builder.Services.AddScoped<UrlService>();

// Почему: строка подключения вынесена в конфигурацию,
// чтобы можно было менять БД без перекомпиляции.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Почему: используем MariaDB, как требует ТЗ.
// Указываем версию сервера, чтобы EF Core корректно генерировал SQL.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(10, 11, 16))
    );
});

var app = builder.Build();

// Почему: автоматические миграции позволяют запускать проект без ручной подготовки БД.
// Это делает приложение самодостаточным.
using (var scope = app.Services.CreateScope())
    {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Почему: гарантируем актуальную схему БД.
    }

// Почему: в продакшене скрываем детали ошибок и включаем HSTS.
// В разработке — наоборот, оставляем подробные ошибки.
if (!app.Environment.IsDevelopment())
    {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    }

// Почему: перенаправляем HTTP → HTTPS для безопасности.
app.UseHttpsRedirection();

// Почему: включаем отдачу статических файлов (CSS, JS, изображения).
app.UseStaticFiles();

app.UseRouting();

// Почему: структура пайплайна должна поддерживать авторизацию,
// даже если сейчас она не используется.
app.UseAuthorization();

// Почему: включаем атрибутные маршруты (например, [Route("r/{code}")]).
app.MapControllers();

// Почему: маршрут по умолчанию ведёт на список ссылок — это основной экран приложения.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Urls}/{action=Index}/{id?}");

app.Run();
