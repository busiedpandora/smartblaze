using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public class ChatGpt : Chatbot
{
    public ChatGpt(string name, List<string> models) : base(name, models)
    {
    }

    public override async Task<string?> GenerateText(ChatSessionDto chatSessionDto, 
        List<MessageDto> messageDtos, string apiHost, string apiKey, HttpClient httpClient)
    { 
        List<Message> messages = new();
        
        foreach (var messageDto in messageDtos)
        {
            if (messageDto.Role == "assistant")
            {
                TextMessage assistantMessage = new()
                {
                    Contents = messageDto.Content,
                    Role = "assistant"
                };
                
                messages.Add(assistantMessage);
            }
            else if (messageDto.Role == "user")
            {
                if (messageDto.UserImageDtos is null || messageDto.UserImageDtos.Count == 0)
                {
                    TextMessage userMessage = new()
                    {
                        Contents = messageDto.Content,
                        Role = "user"
                    };
                    
                    messages.Add(userMessage);
                }
                else
                {
                    List<Content> contents = new();
                    
                    TextContent textContent = new ();
                    textContent.Text = messageDto.Content;
                    contents.Add(textContent);

                    foreach (var userImageDto in messageDto.UserImageDtos)
                    {
                        ImageContent imageContent = new();
                        
                        ImageUrl imageUrl = new();
                        if (userImageDto.Type == "image-file")
                        {
                            imageUrl.Url = $"data:{userImageDto.ContentType};base64,{userImageDto.Content}";
                        }
                        else
                        {
                            imageUrl.Url = userImageDto.Content;
                        }

                        imageContent.ImageUrl = imageUrl;
                        
                        contents.Add(imageContent);
                    }

                    TextImagesMessage userMessage = new()
                    {
                        Contents = (object[]) contents.ToArray(),
                        Role = "user"
                    };
                    
                    messages.Add(userMessage);
                }
            }
        }

        if (!String.IsNullOrEmpty(chatSessionDto.SystemInstruction))
        {
            TextMessage systemInstructionMessage = new()
            {
                Role = "system",
                Contents = chatSessionDto.SystemInstruction
            };

            messages.Insert(0, systemInstructionMessage);
        }

        var chatRequest = new ChatRequest
        {
            Model = chatSessionDto.ChatbotModel,
            Messages = (object[]) messages.ToArray(),
            Stream = false
        };
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var chatRequestJson = JsonSerializer.Serialize(chatRequest, options);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{apiHost}/v1/chat/completions"),
            Headers =
            {
                { "Authorization", $"Bearer {apiKey}" },
            },
            Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
        };
        
        var chatResponseMessage = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        var chatResponseMessageContent = await chatResponseMessage.Content.ReadAsStringAsync();

        if (!chatResponseMessage.IsSuccessStatusCode)
        {
            return chatResponseMessageContent;
        }
        
        ChatResponse? chatResponse = JsonSerializer.Deserialize<ChatResponse>(chatResponseMessageContent);
        
        if (chatResponse is not null && chatResponse.Choices is not null && chatResponse.Choices.Count > 0)
        {
            TextMessage? message = chatResponse.Choices[0].Message;

            if (message is not null)
            {
                return message.Contents;
            }
        }
        
        return null;
    }

    public override async IAsyncEnumerable<string> GenerateTextStreamEnabled(ChatSessionDto chatSessionDto, 
        List<MessageDto> messageDtos, string apiHost, string apiKey, HttpClient httpClient)
    {
        List<Message> messages = new();
        
        foreach (var messageDto in messageDtos)
        {
            if (messageDto.Role == "assistant")
            {
                TextMessage assistantMessage = new()
                {
                    Contents = messageDto.Content,
                    Role = "assistant"
                };
                
                messages.Add(assistantMessage);
            }
            else if (messageDto.Role == "user")
            {
                if (messageDto.UserImageDtos is null || messageDto.UserImageDtos.Count == 0)
                {
                    TextMessage userMessage = new()
                    {
                        Contents = messageDto.Content,
                        Role = "user"
                    };
                    
                    messages.Add(userMessage);
                }
                else
                {
                    List<Content> contents = new();
                    
                    TextContent textContent = new ();
                    textContent.Text = messageDto.Content;
                    contents.Add(textContent);

                    foreach (var userImageDto in messageDto.UserImageDtos)
                    {
                        ImageContent imageContent = new();
                        
                        ImageUrl imageUrl = new();
                        if (userImageDto.Type == "image-file")
                        {
                            imageUrl.Url = $"data:{userImageDto.ContentType};base64,{userImageDto.Content}";
                        }
                        else
                        {
                            imageUrl.Url = userImageDto.Content;
                        }

                        imageContent.ImageUrl = imageUrl;
                        
                        contents.Add(imageContent);
                    }

                    TextImagesMessage userMessage = new()
                    {
                        Contents = (object[]) contents.ToArray(),
                        Role = "user"
                    };
                    
                    messages.Add(userMessage);
                }
            }
        }
        
        if (!String.IsNullOrEmpty(chatSessionDto.SystemInstruction))
        {
            TextMessage systemInstructionMessage = new()
            {
                Role = "system",
                Contents = chatSessionDto.SystemInstruction
            };

            messages.Insert(0, systemInstructionMessage);
        }

        var chatRequest = new ChatRequest
        {
            Model = chatSessionDto.ChatbotModel,
            Messages = (object[]) messages.ToArray(),
            Stream = true
        };
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var chatRequestJson = JsonSerializer.Serialize(chatRequest, options);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{apiHost}/v1/chat/completions"),
            Headers =
            {
                { "Authorization", $"Bearer {apiKey}" },
            },
            Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
        };
        
        var chatResponseMessage = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        if (!chatResponseMessage.IsSuccessStatusCode)
        {
            var chatResponseMessageContent = await chatResponseMessage.Content.ReadAsStringAsync();
            yield return chatResponseMessageContent;
            yield break;
        }
        
        var chatResponseStream = await chatResponseMessage.Content.ReadAsStreamAsync();
        var streamReader = new StreamReader(chatResponseStream);

        string line;

        while (!streamReader.EndOfStream)
        {
            line = await streamReader.ReadLineAsync() ?? "";

            if (line != string.Empty && line.StartsWith("data: "))
            {
                string chunk = line.Substring(6).Trim();

                if (chunk != "[DONE]")
                {
                    ChatResponse? chatResponse = JsonSerializer.Deserialize<ChatResponse>(chunk);

                    if (chatResponse is not null && chatResponse.Choices is not null && chatResponse.Choices.Count > 0)
                    {
                        Delta? delta = chatResponse.Choices[0].Delta;

                        if (delta is not null)
                        {
                            yield return delta.Content ?? "";
                        }
                    }
                }
            }
        }
    }

    public override ChatbotConfigurationDto GetDefaultConfiguration()
    {
        return new ChatbotConfigurationDto()
        {
            ChatbotName = "ChatGPT",
            ChatbotModel = "gpt-4o",
            ApiHost = "https://api.openai.com",
            ApiKey = "",
            Selected = true
        };
    }

    private class ImageUrl
    {
        [JsonPropertyName("url")] 
        public string? Url { get; set; }
    }
    
    private class Content {}

    private class TextContent : Content
    {
        [JsonPropertyName("type")] 
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")] 
        public string? Text { get; set; }
    }

    private class ImageContent : Content
    {
        [JsonPropertyName("type")] 
        public string Type { get; set; } = "image_url";
        
        [JsonPropertyName("image_url")]
        public ImageUrl? ImageUrl { get; set; }
    }

    private class Message { }

    private class TextMessage : Message
    {
        [JsonPropertyName("content")]
        public string? Contents { get; set; }
    
        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }

    private class TextImagesMessage : Message
    {
        [JsonPropertyName("content")]
        public object[]? Contents { get; set; }
    
        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }

    private class Delta
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }
    
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private class ChatRequest
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("messages")]
        public object[]? Messages { get; set; }
        
        [JsonPropertyName("stream")]
        public bool? Stream { get; set; }
    }

    private class ChatResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    
        [JsonPropertyName("created")]
        public long? Created { get; set; }
    
        [JsonPropertyName("model")]
        public string? Model { get; set; }
    
        [JsonPropertyName("object")]
        public string? Object { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    
        [JsonPropertyName("index")]
        public int? Index { get; set; }
    
        [JsonPropertyName("message")]
        public TextMessage? Message { get; set; }
        
        [JsonPropertyName("delta")]
        public Delta? Delta { get; set; }
    }
}