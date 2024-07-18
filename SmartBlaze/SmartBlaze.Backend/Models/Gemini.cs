using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public class Gemini : Chatbot
{
    public Gemini(string name, List<string> models) : base(name, models)
    {
    }

    public override async Task<string?> GenerateText(ChatSessionDto chatSessionDto, 
        List<MessageDto> messageDtos, string apiHost, string apiKey, HttpClient httpClient)
    {
        var contents = new List<RequestContent>();

        foreach (var messageDto in messageDtos)
        {
            List<Part> parts = new();
            
            TextPart textPart = new()
            {
                Text = messageDto.Content
            };
            
            parts.Add(textPart);

            if (messageDto.UserImageDtos is not null && messageDto.UserImageDtos.Count > 0)
            {
                foreach (var userImage in messageDto.UserImageDtos)
                {
                    InlineData inlineData = new()
                    {
                        MimeType = "image/jpeg",
                        Data = userImage.Content
                    };

                    InlineDataPart inlineDataPart = new()
                    {
                        InlineData = inlineData
                    };
                    
                    parts.Add(inlineDataPart);
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

        if (!String.IsNullOrEmpty(chatSessionDto.SystemInstruction))
        {
            TextPart textPart = new()
            {
                Text = chatSessionDto.SystemInstruction
            };
            
            SystemInstruction systemInstruction = new SystemInstruction()
            {
                Part = (object) textPart
            };
            
            chatRequest.SystemInstruction = systemInstruction;
        }
        
        var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{apiHost}/v1beta/models/{chatSessionDto.ChatbotModel}:generateContent?key={apiKey}"),
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

    public override async IAsyncEnumerable<string> GenerateTextStreamEnabled(ChatSessionDto chatSessionDto, 
        List<MessageDto> messageDtos, string apiHost, string apiKey, HttpClient httpClient)
    {
        var contents = new List<RequestContent>();

        foreach (var messageDto in messageDtos)
        {
            List<Part> parts = new();
            
            TextPart textPart = new()
            {
                Text = messageDto.Content
            };
            
            parts.Add(textPart);

            if (messageDto.UserImageDtos is not null && messageDto.UserImageDtos.Count > 0)
            {
                foreach (var userImage in messageDto.UserImageDtos)
                {
                    InlineData inlineData = new()
                    {
                        MimeType = "image/jpeg",
                        Data = userImage.Content
                    };

                    InlineDataPart inlineDataPart = new()
                    {
                        InlineData = inlineData
                    };
                    
                    parts.Add(inlineDataPart);
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

        if (!String.IsNullOrEmpty(chatSessionDto.SystemInstruction))
        {
            TextPart textPart = new()
            {
                Text = chatSessionDto.SystemInstruction
            };
            
            SystemInstruction systemInstruction = new SystemInstruction()
            {
                Part = (object) textPart
            };
            
            chatRequest.SystemInstruction = systemInstruction;
        }
        
        var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{apiHost}/v1beta/models/{chatSessionDto.ChatbotModel}:streamGenerateContent?key={apiKey}"),
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

    public override ChatbotConfigurationDto GetDefaultConfiguration()
    {
        return new ChatbotConfigurationDto()
        {
            ChatbotName = "Google Gemini",
            ChatbotModel = "gemini-1.5-pro",
            ApiHost = "https://generativelanguage.googleapis.com",
            ApiKey = "",
            Selected = false
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