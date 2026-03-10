namespace UrlShortener.Web.Models
    {
    public class ShortUrl
        {
        public int Id { get; set; }

        // Длинный URL, который ввёл пользователь
        public string LongUrl { get; set; } = null!;

        // Уникальный короткий код (часть короткой ссылки)
        public string ShortCode { get; set; } = null!;

        // Дата создания записи
        public DateTime CreatedAt { get; set; }

        // Количество переходов по короткой ссылке
        public int Clicks { get; set; }
        }
    }
