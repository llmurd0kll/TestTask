using Microsoft.EntityFrameworkCore;
using UrlShortener.Web.Data;
using UrlShortener.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем MVC
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<UrlService>();

// Подключаем DbContext с SQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(10, 11, 16)) // версия MariaDB
    );
});

var app = builder.Build();

// Автоматическое применение миграций при старте
using (var scope = app.Services.CreateScope())
    {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    }

// Стандартный пайплайн
if (!app.Environment.IsDevelopment())
    {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    }

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Маршруты MVC
// ВАЖНО: включаем атрибутные маршруты
app.MapControllers();

// ВАЖНО: обычный MVC маршрут
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Urls}/{action=Index}/{id?}");

app.Run();
