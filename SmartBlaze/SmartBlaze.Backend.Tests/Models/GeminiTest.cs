using JetBrains.Annotations;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Tests.Models;

[TestSubject(typeof(Gemini))]
public class GeminiTest
{
    [Fact]
    public void GetDefaultConfiguration()
    {
        var gemini = new Gemini("Google Gemini", [], [], false);

        var result = gemini.GetDefaultConfiguration();
        
        Assert.Equal("Google Gemini",result.ChatbotName);
        Assert.Equal("gemini-1.5-flash",result.TextGenerationChatbotModel);
        Assert.Equal("https://generativelanguage.googleapis.com",result.ApiHost);
        Assert.Equal("",result.ApiKey);
        Assert.False(result.Selected);
        Assert.Equal(1.0f, result.Temperature);
    }
}