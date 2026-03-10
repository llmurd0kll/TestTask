using UrlShortener.Web.Services;
using Xunit;

public class CodeGeneratorTests
    {
    [Fact]
    public void GenerateShortCode_ShouldReturn8CharString()
        {
        var code = CodeGenerator.GenerateShortCode();

        Assert.Equal(8, code.Length);
        }
    }
