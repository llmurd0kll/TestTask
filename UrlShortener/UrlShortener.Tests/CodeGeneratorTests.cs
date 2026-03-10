using UrlShortener.Web.Services;

public class CodeGeneratorTests
    {
    [Fact]
    public void GenerateShortCode_ShouldReturn10CharString()
        {
        // Почему: длина кода увеличена с 8 до 10 символов,
        // чтобы снизить вероятность коллизий при высокой нагрузке
        // (1000+ запросов в секунду и миллионы записей в БД).
        var code = CodeGenerator.GenerateShortCode();

        Assert.Equal(10, code.Length);
        }
    }
