using JetBrains.Annotations;
using SmartBlaze.Backend.Models;

namespace SmartBlaze.Backend.Tests.Models;

[TestSubject(typeof(ChatGpt))]
public class ChatGptTest
{
    [Fact]
    public void GetDefaultConfiguration()
    {
        var chatGpt = new ChatGpt("Google Gemini", [], [], false);

        var result = chatGpt.GetDefaultConfiguration();
        
        Assert.Equal("ChatGPT",result.ChatbotName);
        Assert.Equal("gpt-4o",result.TextGenerationChatbotModel);
        Assert.Equal("dall-e-3",result.ImageGenerationChatbotModel);
        Assert.Equal("https://api.openai.com",result.ApiHost);
        Assert.Equal("",result.ApiKey);
        Assert.True(result.Selected);
        Assert.Equal(1.0f, result.Temperature);
    }
}