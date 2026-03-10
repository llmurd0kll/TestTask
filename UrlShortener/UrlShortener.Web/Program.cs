using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Data;
using UrlShortener.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Почему: MVC нужен, чтобы приложение могло работать с контроллерами и представлениями.
// Без этого не будут работать маршруты, страницы и контроллеры.
builder.Services.AddControllersWithViews();

// Почему: UrlService — бизнес-логика приложения.
// Регистрируем как Scoped, чтобы на каждый HTTP‑запрос был свой экземпляр.
builder.Services.AddScoped<UrlService>();

// Почему: строка подключения вынесена в конфигурацию,
// чтобы можно было менять БД без перекомпиляции проекта.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Почему: используем MySQL/MariaDB, как требует ТЗ.
// DbContext регистрируется здесь, чтобы EF Core мог работать с БД.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(10, 11, 16)) // Почему: указываем версию MariaDB, иначе EF может работать некорректно.
    );
});

var app = builder.Build();

// Почему: автоматические миграции позволяют запускать проект "из коробки",
// без ручного применения SQL-скриптов. Это требование из вашего ТЗ.
using (var scope = app.Services.CreateScope())
    {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Почему: гарантируем, что БД существует и актуальна.
    }

// Почему: в продакшене нужно скрывать ошибки и включать HSTS для безопасности.
// В режиме разработки это отключено, чтобы удобнее было отлаживать.
if (!app.Environment.IsDevelopment())
    {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    }

// Почему: перенаправляем HTTP → HTTPS, чтобы избежать небезопасных соединений.
app.UseHttpsRedirection();

// Почему: включаем отдачу статических файлов (CSS, JS, изображения).
app.UseStaticFiles();

app.UseRouting();

// Почему: если появится авторизация — она будет работать.
// Сейчас она не используется, но структура пайплайна должна быть корректной.
app.UseAuthorization();

// Почему: включаем атрибутные маршруты.
// Без этого не будет работать маршрут вида [Route("r/{code}")].
app.MapControllers();

// Почему: задаём маршрут по умолчанию, чтобы сайт открывался на списке ссылок.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Urls}/{action=Index}/{id?}");

app.Run();
