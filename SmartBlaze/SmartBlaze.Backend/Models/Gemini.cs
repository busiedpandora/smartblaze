using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public class Gemini : Chatbot
{
    public Gemini(string name, List<string> textGenerationModels, List<string> imageGenerationModels) 
        : base(name, textGenerationModels, imageGenerationModels)
    {
    }

    public override async Task<string?> GenerateText(ChatSessionInfoDto chatSessionInfoDto, HttpClient httpClient)
    {
        var contents = new List<RequestContent>();

        foreach (var messageDto in chatSessionInfoDto.Messages)
        {
            List<Part> parts = new();
            
            TextPart textPart = new()
            {
                Text = messageDto.Text
            };
            
            parts.Add(textPart);

            if (messageDto.MediaDtos is not null && messageDto.MediaDtos.Count > 0)
            {
                foreach (var mediaDto in messageDto.MediaDtos)
                {
                    if (mediaDto.ContentType.StartsWith("image"))
                    {
                        InlineData inlineData = new()
                        {
                            MimeType = mediaDto.ContentType,
                            Data = mediaDto.Data
                        };

                        InlineDataPart inlineDataPart = new()
                        {
                            InlineData = inlineData
                        };
                        
                        parts.Add(inlineDataPart);
                    }
                    else
                    {
                        textPart.Text += $"\n```{mediaDto.ContentType}\n{mediaDto.Data}\n```";
                    }
                }
            }
            
            string? role = messageDto.Role;

            if (role == Role.Assistant)
            {
                role = "model";
            }

            RequestContent content = new()
            {
                Role = role,
                Parts = (object[]) parts.ToArray()
            };
            
            contents.Add(content);
        }

        var chatRequest = new ChatRequest
        {
            Contents = contents
        };

        if (!String.IsNullOrEmpty(chatSessionInfoDto.SystemInstruction))
        {
            TextPart textPart = new()
            {
                Text = chatSessionInfoDto.SystemInstruction
            };
            
            SystemInstruction systemInstruction = new SystemInstruction()
            {
                Part = (object) textPart
            };
            
            chatRequest.SystemInstruction = systemInstruction;
        }
        
        GenerationConfig generationConfig = new()
        {
            Temperature = chatSessionInfoDto.Temperature
        };

        chatRequest.GenerationConfig = generationConfig;
        
        var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{chatSessionInfoDto.ApiHost}/v1beta/models/" +
                                 $"{chatSessionInfoDto.ChatbotModel}:generateContent?key={chatSessionInfoDto.ApiKey}"),
            Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
        };
        
        var chatResponseMessage = await httpClient.SendAsync(httpRequest);
        var chatResponseMessageContent = await chatResponseMessage.Content.ReadAsStringAsync();

        if (!chatResponseMessage.IsSuccessStatusCode)
        {
            return chatResponseMessageContent;
        }
        
        ChatResponse? chatResponse = JsonSerializer.Deserialize<ChatResponse>(chatResponseMessageContent);

        if (chatResponse is not null && chatResponse.Candidates is not null)
        {
            var candidate = chatResponse.Candidates[0];

            if (candidate.Content is not null && candidate.Content.Parts is not null)
            {
                return candidate.Content.Parts[0].Text;
            }
        }

        return null;
    }

    public override async IAsyncEnumerable<string> GenerateTextStreamEnabled(ChatSessionInfoDto chatSessionInfoDto,
        HttpClient httpClient)
    {
        var contents = new List<RequestContent>();

        foreach (var messageDto in chatSessionInfoDto.Messages)
        {
            List<Part> parts = new();
            
            TextPart textPart = new()
            {
                Text = messageDto.Text
            };
            
            parts.Add(textPart);

            if (messageDto.MediaDtos is not null && messageDto.MediaDtos.Count > 0)
            {
                foreach (var mediaDto in messageDto.MediaDtos)
                {
                    if (mediaDto.ContentType.StartsWith("image"))
                    {
                        InlineData inlineData = new()
                        {
                            MimeType = mediaDto.ContentType,
                            Data = mediaDto.Data
                        };

                        InlineDataPart inlineDataPart = new()
                        {
                            InlineData = inlineData
                        };
                        
                        parts.Add(inlineDataPart);
                    }
                    else
                    {
                        textPart.Text += $"\n```{mediaDto.ContentType}\n{mediaDto.Data}\n```";
                    }
                }
            }
            
            string? role = messageDto.Role;

            if (role == Role.Assistant)
            {
                role = "model";
            }

            RequestContent content = new()
            {
                Role = role,
                Parts = (object[]) parts.ToArray()
            };
            
            contents.Add(content);
        }

        var chatRequest = new ChatRequest
        {
            Contents = contents
        };

        if (!String.IsNullOrEmpty(chatSessionInfoDto.SystemInstruction))
        {
            TextPart textPart = new()
            {
                Text = chatSessionInfoDto.SystemInstruction
            };
            
            SystemInstruction systemInstruction = new SystemInstruction()
            {
                Part = (object) textPart
            };
            
            chatRequest.SystemInstruction = systemInstruction;
        }

        GenerationConfig generationConfig = new()
        {
            Temperature = chatSessionInfoDto.Temperature
        };

        chatRequest.GenerationConfig = generationConfig;
        
        var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{chatSessionInfoDto.ApiHost}/v1beta/models/" +
                                 $"{chatSessionInfoDto.ChatbotModel}:streamGenerateContent?key={chatSessionInfoDto.ApiKey}"),
            Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
        };
        
        var chatResponseMessage = await httpClient.SendAsync(httpRequest);

        if (!chatResponseMessage.IsSuccessStatusCode)
        {
            var chatResponseMessageContent = await chatResponseMessage.Content.ReadAsStringAsync();
            yield return chatResponseMessageContent;
            yield break;
        }
        
        var chatResponseStream = await chatResponseMessage.Content.ReadAsStreamAsync();
        var streamReader = new StreamReader(chatResponseStream);

        string? line;
        
        while ((line = await streamReader.ReadLineAsync()) != null)
        {
            line = line.Trim();
            if (line.StartsWith("\"text\":"))
            {
                line = "{" + line + "}";
                TextPart? part = JsonSerializer.Deserialize<TextPart>(line);

                if (part is not null)
                {
                    yield return part.Text ?? "";
                }
            }
        }
    }

    public override Task<AssistantMessageInfoDto> GenerateImage(ChatSessionInfoDto chatSessionInfoDto, HttpClient httpClient)
    {
        throw new NotImplementedException();
    }

    public override ChatbotDefaultConfigurationDto GetDefaultConfiguration()
    {
        return new ChatbotDefaultConfigurationDto()
        {
            ChatbotName = "Google Gemini",
            TextGenerationChatbotModel = "gemini-1.5-pro",
            ApiHost = "https://generativelanguage.googleapis.com",
            ApiKey = "",
            TextStreamDelay = 400,
            Selected = false,
            Temperature = 1.0f,
            MinTemperature = 0.0f,
            MaxTemperature = 2.0f
        };
    }

    private class Part { }

    private class TextPart : Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class InlineDataPart : Part
    {
        [JsonPropertyName("inlineData")]
        public InlineData? InlineData { get; set; }
    }

    private class InlineData
    {
        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }
        
        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }

    private class GenerationConfig
    {
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }
    }
    
    private class RequestContent
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }
        
        [JsonPropertyName("parts")]
        public object[]? Parts { get; set; }
    }

    private class ResponseContent
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }
        
        [JsonPropertyName("parts")]
        public List<TextPart>? Parts { get; set; }
    }

    private class SystemInstruction
    {
        [JsonPropertyName("parts")]
        public object? Part { get; set; }
    }
    
    private class ChatRequest
    {
        [JsonPropertyName("system_instruction")]
        public SystemInstruction? SystemInstruction { get; set; }
        
        [JsonPropertyName("contents")]
        public List<RequestContent>? Contents { get; set; }
        
        [JsonPropertyName("generationConfig")]
        public GenerationConfig? GenerationConfig { get; set; }
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public ResponseContent? Content { get; set; }
        
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    
        [JsonPropertyName("index")]
        public int? Index { get; set; }
    }

    private class ChatResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }
}