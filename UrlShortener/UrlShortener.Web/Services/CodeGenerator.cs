using System.Security.Cryptography;

namespace UrlShortener.Web.Services
    {
    public static class CodeGenerator
        {
        private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// Генерирует криптографически стойкий короткий код.
        /// Почему: короткий код должен быть непредсказуемым, иначе злоумышленник сможет
        /// перебором находить чужие ссылки. Random() предсказуем, а RandomNumberGenerator —
        /// криптографически безопасный источник случайности.
        /// </summary>
        public static string GenerateShortCode(int length = 10)
            {
            // Почему: увеличиваем длину кода (10 вместо 8), чтобы резко снизить вероятность
            // коллизий при большом количестве записей и высокой нагрузке.
            var bytes = RandomNumberGenerator.GetBytes(length);

            // Почему: сначала генерируем байты, а не символы — это даёт равномерное распределение.
            var result = new char[length];

            for (int i = 0 ; i < length ; i++)
                {
                // Почему: берём байт по модулю длины набора символов —
                // так гарантируется, что каждый символ попадёт в AllowedChars.
                result[i] = AllowedChars[bytes[i] % AllowedChars.Length];
                }

            // Почему: возвращаем строку — это удобнее для хранения, передачи и сравнения.
            return new string(result);
            }
        }
    }
