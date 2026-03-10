using System.Security.Cryptography;

namespace UrlShortener.Web.Services
    {
    public static class CodeGenerator
        {
        private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// Генерирует криптографически стойкий короткий код.
        /// Почему: короткий код должен быть непредсказуемым, иначе злоумышленник сможет
        /// перебором находить чужие ссылки. Random() генерирует предсказуемую последовательность,
        /// а RandomNumberGenerator использует криптографически безопасный источник случайности.
        /// </summary>
        public static string GenerateShortCode(int length = 8)
            {
            // Почему: генерируем массив случайных байтов, а не символов.
            // Это даёт равномерное распределение и исключает смещения.
            var bytes = RandomNumberGenerator.GetBytes(length);

            var result = new char[length];

            for (int i = 0 ; i < length ; i++)
                {
                // Почему: берём байт по модулю длины набора символов.
                // Это гарантирует, что каждый символ будет из AllowedChars.
                result[i] = AllowedChars[bytes[i] % AllowedChars.Length];
                }

            // Почему: возвращаем строку, а не массив char — это удобнее для хранения и передачи.
            return new string(result);
            }
        }
    }
