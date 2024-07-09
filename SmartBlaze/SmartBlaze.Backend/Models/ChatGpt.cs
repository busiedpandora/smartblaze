using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartBlaze.Backend.Dtos;

namespace SmartBlaze.Backend.Models;

public class ChatGpt : Chatbot
{
    public ChatGpt(string name, string apiHost, string apiKey) : base(name, apiHost, apiKey)
    {
    }

    public override async Task<string?> GenerateText(ChatSessionDto chatSessionDto, 
        List<MessageDto> messageDtos, HttpClient httpClient)
    {
        var messages = messageDtos
            .Select(m => new Message
            {
                Content = m.Content,
                Role = m.Role
            })
            .ToList();

        if (!String.IsNullOrEmpty(chatSessionDto.SystemInstruction))
        {
            Message systemInstructionMessage = new Message()
            {
                Role = "system",
                Content = chatSessionDto.SystemInstruction
            };

            messages.Insert(0, systemInstructionMessage);
        }

        var chatRequest = new ChatRequest
        {
            Model = chatSessionDto.ChatbotModel,
            Messages = messages,
            Stream = false
        };

        var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{ApiHost}/v1/chat/completions"),
            Headers =
            {
                { "Authorization", $"Bearer {ApiKey}" },
            },
            Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
        };
        
        var chatResponseMessage = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        chatResponseMessage.EnsureSuccessStatusCode();
        
        var chatResponseString = await chatResponseMessage.Content.ReadAsStringAsync();
        
        ChatResponse? chatResponse = JsonSerializer.Deserialize<ChatResponse>(chatResponseString);
        
        if (chatResponse is not null && chatResponse.Choices is not null && chatResponse.Choices.Count > 0)
        {
            Message? message = chatResponse.Choices[0].Message;

            if (message is not null)
            {
                return message.Content;
            }
        }
        
        return null;
    }

    public override async IAsyncEnumerable<string> GenerateTextStreamEnabled(ChatSessionDto chatSessionDto, List<MessageDto> messageDtos, 
        HttpClient httpClient)
    {
        var messages = messageDtos
            .Select(m => new Message
            {
                Content = m.Content,
                Role = m.Role
            })
            .ToList();
        
        if (!String.IsNullOrEmpty(chatSessionDto.SystemInstruction))
        {
            Message systemInstructionMessage = new Message()
            {
                Role = "system",
                Content = chatSessionDto.SystemInstruction
            };
        }

        var chatRequest = new ChatRequest
        {
            Model = chatSessionDto.ChatbotModel,
            Messages = messages,
            Stream = true
        };

        var chatRequestJson = JsonSerializer.Serialize(chatRequest);
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{ApiHost}/v1/chat/completions"),
            Headers =
            {
                { "Authorization", $"Bearer {ApiKey}" },
            },
            Content = new StringContent(chatRequestJson, Encoding.UTF8, "application/json")
        };
        
        var chatResponseMessage = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        chatResponseMessage.EnsureSuccessStatusCode();
        
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

    private class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    
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
        public List<Message>? Messages { get; set; }
        
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
        public Message? Message { get; set; }
        
        [JsonPropertyName("delta")]
        public Delta? Delta { get; set; }
    }
}