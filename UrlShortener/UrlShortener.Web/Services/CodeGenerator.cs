using System.Security.Cryptography;

namespace UrlShortener.Web.Services
    {
    public static class CodeGenerator
        {
        private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// Генерирует криптографически стойкий короткий код.
        /// Почему так: Random() предсказуем, а RandomNumberGenerator — нет.
        /// </summary>
        public static string GenerateShortCode(int length = 8)
            {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var result = new char[length];

            for (int i = 0 ; i < length ; i++)
                {
                result[i] = AllowedChars[bytes[i] % AllowedChars.Length];
                }

            return new string(result);
            }
        }
    }
