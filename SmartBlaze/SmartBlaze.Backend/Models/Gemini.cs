using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public class Gemini : Chatbot
{
    public Gemini(string name, 
        List<TextGenerationChatbotModel> textGenerationChatbotModels, List<ImageGenerationChatbotModel> imageGenerationChatbotModels,
        bool supportImageGeneration) 
        : base(name, textGenerationChatbotModels, imageGenerationChatbotModels, supportImageGeneration)
    {
    }

    public override async Task<AssistantMessageInfoDto> GenerateText(TextGenerationRequestData textGenerationRequestData, HttpClient httpClient)
    {
        var contents = new List<RequestContent>();

        foreach (var messageDto in textGenerationRequestData.Messages)
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
                        textPart.Text += $"\n```{mediaDto.ContentType}\nfile name: {mediaDto.Name}\n{mediaDto.Data}\n```";
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

        if (textGenerationRequestData.ChatbotModel.AcceptSystemInstruction && 
            !String.IsNullOrEmpty(textGenerationRequestData.SystemInstruction))
        {
            TextPart textPart = new()
            {
                Text = textGenerationRequestData.SystemInstruction
            };
            
            SystemInstruction systemInstruction = new SystemInstruction()
            {
                Part = (object) textPart
            };
            
            chatRequest.SystemInstruction = systemInstruction;
        }
        
        GenerationConfig generationConfig = new()
        {
            Temperature = textGenerationRequestData.Temperature
        };

        chatRequest.GenerationConfig = generationConfig;
        
        var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{textGenerationRequestData.ApiHost}/v1beta/models/" +
                                 $"{textGenerationRequestData.ChatbotModel.Name}:generateContent?key={textGenerationRequestData.ApiKey}"),
            Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
        };
        
        var chatResponseMessage = await httpClient.SendAsync(httpRequest);
        var chatResponseMessageContent = await chatResponseMessage.Content.ReadAsStringAsync();

        if (!chatResponseMessage.IsSuccessStatusCode)
        {
            return new AssistantMessageInfoDto
            {
                Status = "error",
                Text = chatResponseMessageContent
            };
        }
        
        ChatResponse? chatResponse = JsonSerializer.Deserialize<ChatResponse>(chatResponseMessageContent);

        if (chatResponse is not null && chatResponse.Candidates is not null)
        {
            var candidate = chatResponse.Candidates[0];

            if (candidate.Content is not null && candidate.Content.Parts is not null)
            {
                return new AssistantMessageInfoDto
                {
                    Status = "ok",
                    Text = candidate.Content.Parts[0].Text
                };
            }
        }

        return new AssistantMessageInfoDto()
        {
            Status = "error",
            Text = "No content has been generated"
        };
    }

    public override async IAsyncEnumerable<string> GenerateTextStreamEnabled(TextGenerationRequestData textGenerationRequestData,
        HttpClient httpClient)
    {
        var contents = new List<RequestContent>();

        foreach (var messageDto in textGenerationRequestData.Messages)
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
                        textPart.Text += $"\n```{mediaDto.ContentType}\nfile name: {mediaDto.Name}\n{mediaDto.Data}\n```";
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

        if (textGenerationRequestData.ChatbotModel.AcceptSystemInstruction && 
            !String.IsNullOrEmpty(textGenerationRequestData.SystemInstruction))
        {
            TextPart textPart = new()
            {
                Text = textGenerationRequestData.SystemInstruction
            };
            
            SystemInstruction systemInstruction = new SystemInstruction()
            {
                Part = (object) textPart
            };
            
            chatRequest.SystemInstruction = systemInstruction;
        }

        GenerationConfig generationConfig = new()
        {
            Temperature = textGenerationRequestData.Temperature
        };

        chatRequest.GenerationConfig = generationConfig;
        
        var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{textGenerationRequestData.ApiHost}/v1beta/models/" +
                                 $"{textGenerationRequestData.ChatbotModel.Name}:streamGenerateContent?key={textGenerationRequestData.ApiKey}"),
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

    public override async Task<AssistantMessageInfoDto> GenerateImage(ImageGenerationRequestData imageGenerationRequestData, 
        HttpClient httpClient)
    {
        if (!SupportImageGeneration)
        {
            return new AssistantMessageInfoDto()
            {
                Status = "error",
                Text = "Currently, ChatGPT is not able to generate images"
            };
        }

        return new AssistantMessageInfoDto()
        {
            Status = "ok"
        };
    }

    public override async Task<AssistantMessageInfoDto> EntitleChatSession(TextGenerationRequestData textGenerationRequestData, HttpClient httpClient)
    {
        var contents = new List<RequestContent>();

        if (textGenerationRequestData.Messages.Count > 0)
        {
            var userMessageDto = textGenerationRequestData.Messages.ElementAt(0);

            if (!textGenerationRequestData.ChatbotModel.AcceptSystemInstruction)
            {
                userMessageDto.Text = textGenerationRequestData.SystemInstruction + " Here is the text content: " +
                                      userMessageDto.Text;
            }
            
            List<Part> parts = new();
            
            TextPart textPart = new()
            {
                Text = userMessageDto.Text
            };
            
            parts.Add(textPart);
            
            RequestContent content = new()
            {
                Role = "user",
                Parts = (object[]) parts.ToArray()
            };
            
            contents.Add(content);
            
            var chatRequest = new ChatRequest
            {
                Contents = contents
            };

            if (textGenerationRequestData.ChatbotModel.AcceptSystemInstruction)
            {
                TextPart systemInstructionTextPart = new()
                {
                    Text = textGenerationRequestData.SystemInstruction
                };
            
                SystemInstruction systemInstruction = new SystemInstruction()
                {
                    Part = (object) systemInstructionTextPart
                };
            
                chatRequest.SystemInstruction = systemInstruction;
            }
            
            var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{textGenerationRequestData.ApiHost}/v1beta/models/" +
                                     $"{textGenerationRequestData.ChatbotModel.Name}:generateContent?key={textGenerationRequestData.ApiKey}"),
                Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
            };
        
            var chatResponseMessage = await httpClient.SendAsync(httpRequest);
            var chatResponseMessageContent = await chatResponseMessage.Content.ReadAsStringAsync();
            
            if (!chatResponseMessage.IsSuccessStatusCode)
            {
                return new AssistantMessageInfoDto
                {
                    Status = "error",
                    Text = chatResponseMessageContent
                };
            }
        
            ChatResponse? chatResponse = JsonSerializer.Deserialize<ChatResponse>(chatResponseMessageContent);

            if (chatResponse is not null && chatResponse.Candidates is not null)
            {
                var candidate = chatResponse.Candidates[0];

                if (candidate.Content is not null && candidate.Content.Parts is not null)
                {
                    return new AssistantMessageInfoDto
                    {
                        Status = "ok",
                        Text = candidate.Content.Parts[0].Text?.Trim()
                    };
                }
            }
        }

        return new AssistantMessageInfoDto()
        {
            Status = "error",
            Text = "No content has been generated"
        };
    }

    public override ChatbotDefaultConfigurationDto GetDefaultConfiguration()
    {
        return new ChatbotDefaultConfigurationDto()
        {
            ChatbotName = "Google Gemini",
            TextGenerationChatbotModel = "gemini-1.5-flash",
            ApiHost = "https://generativelanguage.googleapis.com",
            ApiKey = "",
            Selected = false,
            Temperature = 1.0f
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