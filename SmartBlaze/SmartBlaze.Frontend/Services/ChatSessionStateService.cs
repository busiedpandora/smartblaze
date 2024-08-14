using System.Text.Json;
using SmartBlaze.Frontend.Dtos;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class ChatSessionStateService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private SettingsService _settingsService;
    
    private List<ChatSessionDto>? _chatSessions;
    private ChatSessionDto? _currentChatSession;
    private List<MessageDto>? _currentChatSessionMessages;
    private ChatSessionConfigurationDto? _currentChatSessionConfiguration;

    private bool _areChatSessionsLoadingOnStartup;
    private bool _isNewChatSessionBeingCreated;
    private bool _isChatSessionBeingSelected;
    private bool _isGeneratingResponse;
    private bool _isChatSessionBeingDeleted;
    private bool _isChatSessionBeingEdited;

    private string _currentGenerationType = "text";


    public ChatSessionStateService(IHttpClientFactory httpClientFactory, SettingsService settingsService) 
        : this(httpClientFactory)
    {
        _settingsService = settingsService;
    }

    public List<ChatSessionDto>? ChatSessions
    {
        get => _chatSessions;
    }

    public ChatSessionDto? CurrentChatSession
    {
        get => _currentChatSession;
    }

    public List<MessageDto>? CurrentChatSessionMessages
    {
        get => _currentChatSessionMessages;
    }

    public ChatSessionConfigurationDto? CurrentChatSessionConfiguration => _currentChatSessionConfiguration;

    public bool AreChatSessionsLoadingOnStartup
    {
        get => _areChatSessionsLoadingOnStartup;
        set => _areChatSessionsLoadingOnStartup = value;
    }

    public bool IsNewChatSessionBeingCreated
    {
        get => _isNewChatSessionBeingCreated;
    }

    public bool IsChatSessionBeingSelected
    {
        get => _isChatSessionBeingSelected;
    }

    public bool IsGeneratingResponse
    {
        get => _isGeneratingResponse;
    }

    public bool IsChatSessionBeingDeleted => _isChatSessionBeingDeleted;

    public string CurrentGenerationType
    {
        get => _currentGenerationType;
        set => _currentGenerationType = value ?? throw new ArgumentNullException(nameof(value));
    }

    public async Task SelectChatSession(ChatSessionDto chatSession)
    {
        if (_chatSessions is null)
        {
            return;
        }
        
        if (!CanUserInteract())
        {
            return;
        }

        if (_currentChatSession is not null && _currentChatSession.Id == chatSession.Id)
        {
            return;
        }

        _isChatSessionBeingSelected = true;
        NotifyRefreshView();
        
        ChatSessionDto? newSelectedChatSession = _chatSessions.Find(c => c.Id == chatSession.Id);

        if (newSelectedChatSession is null)
        {
            _isChatSessionBeingSelected = false;
            HandleError("Error occured while selecting the chat session", $"chat session with id {chatSession.Id} not found");
            return;
        }
        
        var chatSessionConfigurationResponse = await HttpClient.GetAsync($"configuration/chat-session/{newSelectedChatSession.Id}");
        var chatSessionConfigurationResponseContent =
            await chatSessionConfigurationResponse.Content.ReadAsStringAsync();

        if (!chatSessionConfigurationResponse.IsSuccessStatusCode)
        {
            _isChatSessionBeingSelected = false;
            HandleError($"Error occured while selecting the chat session {chatSession.Title}", 
                chatSessionConfigurationResponseContent);
            return;
        }

        var chatSessionConfiguration =
            JsonSerializer.Deserialize<ChatSessionConfigurationDto>(chatSessionConfigurationResponseContent);

        if (chatSessionConfiguration is not null)
        {
            var chatbot = _settingsService.GetChatbotByName(chatSessionConfiguration.ChatbotName);
            if (chatbot is not null)
            {
                chatSessionConfiguration.SupportBase64InputImageFormat = chatbot.SupportBase64ImageInputFormat;
                chatSessionConfiguration.SupportUrlInputImageFormat = chatbot.SupportUrlImageInputFormat;
                chatSessionConfiguration.SupportImageGeneration = chatbot.SupportImageGeneration;
            }
        }

        _currentChatSessionConfiguration = chatSessionConfiguration;
        
        var response = await HttpClient.GetAsync($"chat-session/{newSelectedChatSession.Id}/messages");
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _isChatSessionBeingSelected = false;
            HandleError("Error occured while selecting the chat session", responseContent);
            return;
        }
        
        if (responseContent == string.Empty)
        {
            _currentChatSessionMessages = new List<MessageDto>();
            
        }
        else
        {
            List<MessageDto>? messagesDto = JsonSerializer.Deserialize<List<MessageDto>>(responseContent) ?? new List<MessageDto>();
            _currentChatSessionMessages = messagesDto;
        }
        
        newSelectedChatSession.Selected = true;

        if (_currentChatSession is not null)
        {
            _currentChatSession.Selected = false;
        }

        _currentChatSession = newSelectedChatSession;
        
        NotifyNavigateToPage("/");

        _isChatSessionBeingSelected = false;
        
        NotifyRefreshView();
    }

    public void DeselectCurrentChatSession()
    {
        if (_chatSessions is null)
        {
            return;
        }
        
        if (_currentChatSession is not null)
        {
            _currentChatSession.Selected = false;

            _currentChatSession = null;
            _currentChatSessionMessages = null;
            _currentChatSessionConfiguration = null;
            
            NotifyRefreshView();
        }
    }

    public async Task RequestNewAssistantTextMessage(string text, List<MediaDto> fileInputs, string apiHost, string apiKey, 
        int textStreamDelay)
    {
        if (_chatSessions is null || _currentChatSession is null || _currentChatSessionMessages is null 
            || _currentChatSessionConfiguration is null)
        {
            return;
        }
        
        if (!CanUserInteract())
        {
            return;
        }

        _isGeneratingResponse = true;

        var userMessage = await SendUserMessage(text, fileInputs);
        _currentChatSessionMessages.Add(userMessage);

        if (userMessage.Status == "error")
        {
            _isGeneratingResponse = false;
            NotifyRefreshView();
            return;
        }
        
        NotifyRefreshView();

        var chatSessionInfoDto = new ChatSessionInfoDto()
        {
            Messages = _currentChatSessionMessages,
            ChatbotName = _currentChatSessionConfiguration.ChatbotName,
            ChatbotModel = _currentChatSessionConfiguration.TextGenerationChatbotModel,
            ApiHost = apiHost,
            ApiKey = apiKey,
            SystemInstruction = _currentChatSessionConfiguration.SystemInstruction,
            Temperature = _currentChatSessionConfiguration.Temperature,
            TextStreamDelay = textStreamDelay
        };
        
        if (_currentChatSessionConfiguration.TextStream)
        {
            await GenerateAssistantTextMessageWithStreamEnabled(chatSessionInfoDto);
        }
        else
        {
            var assistantMessage = await GenerateAssistantTextMessage(chatSessionInfoDto);
            _currentChatSessionMessages.Add(assistantMessage);
        }
        
        _isGeneratingResponse = false;
        
        NotifyRefreshView();
    }

    public async Task RequestNewAssistantImageMessage(string text, string apiHost, string apiKey)
    {
        if (_chatSessions is null || _currentChatSession is null || _currentChatSessionMessages is null 
            || _currentChatSessionConfiguration is null)
        {
            return;
        }
        
        if (!CanUserInteract())
        {
            return;
        }

        _isGeneratingResponse = true;

        var userMessage = await SendUserMessage(text);
        _currentChatSessionMessages.Add(userMessage);
        
        if (userMessage.Status == "error")
        {
            _isGeneratingResponse = false;
            NotifyRefreshView();
            return;
        }
        
        NotifyRefreshView();
        
        var chatSessionInfoDto = new ChatSessionInfoDto()
        {
            LastUserMessage = _currentChatSessionMessages.Last(),
            ChatbotName = _currentChatSessionConfiguration.ChatbotName,
            ChatbotModel = _currentChatSessionConfiguration.ImageGenerationChatbotModel,
            ApiHost = apiHost,
            ApiKey = apiKey,
        };

        var assistantMessage = await GenerateAssistantImageMessage(chatSessionInfoDto);
        _currentChatSessionMessages.Add(assistantMessage);
        
        _isGeneratingResponse = false;
        
        NotifyRefreshView();
    }

    public async Task CreateNewChatSession(string title, string chatbotName, string textGenerationChatbotModel, 
        string imageGenerationChatbotModel, float temperature, string systemInstruction, bool textStream)
    {
        if (!CanUserInteract())
        {
            return;
        }
        
        _isNewChatSessionBeingCreated = true;
        NotifyRefreshView();
        
        if (_chatSessions is null)
        {
            _chatSessions = new List<ChatSessionDto>();
        }
        
        ChatSessionDto? chatSessionDto = new ChatSessionDto();
        chatSessionDto.Title = title;
        
        var newChatSessionResponse = await HttpClient.PostAsJsonAsync("chat-sessions/new", chatSessionDto);
        var newChatSessionResponseContent = await newChatSessionResponse.Content.ReadAsStringAsync();

        if (!newChatSessionResponse.IsSuccessStatusCode)
        {
            _isNewChatSessionBeingCreated = false;
            HandleError("Error occured while creating a new chat session", newChatSessionResponseContent);
            return;
        }
        
        chatSessionDto = JsonSerializer.Deserialize<ChatSessionDto>(newChatSessionResponseContent);

        if (chatSessionDto is null || !IsChatSessionValid(chatSessionDto))
        {
            _isNewChatSessionBeingCreated = false;
            HandleError("Error occured while creating a new chat session", 
                "The assistant message could not be deserialized");
            return;
        }
        
        ChatSessionConfigurationDto chatSessionConfigurationDto = new()
        {
            ChatbotName = chatbotName,
            TextGenerationChatbotModel = textGenerationChatbotModel,
            ImageGenerationChatbotModel = imageGenerationChatbotModel,
            Temperature = temperature,
            SystemInstruction = systemInstruction,
            TextStream = textStream
        };

        var chatSessionConfigurationResponse = await HttpClient.PostAsJsonAsync($"configuration/chat-session/{chatSessionDto.Id}", 
            chatSessionConfigurationDto);
        
        if (!chatSessionConfigurationResponse.IsSuccessStatusCode)
        {
            var chatSessionConfigurationResponseContent = await chatSessionConfigurationResponse.Content.ReadAsStringAsync();
            _isNewChatSessionBeingCreated = false;
            HandleError("Error occured while creating a new chat session", chatSessionConfigurationResponseContent);
            return;
        }
        
        NotifyNavigateToPage("/");
        
        _chatSessions.Insert(0, chatSessionDto);
        _isNewChatSessionBeingCreated = false;
        await SelectChatSession(chatSessionDto);
        NotifyRefreshView();
    }

    public void OpenChatSessionSettings()
    {
        if(_currentChatSession is not null)
        {
            NotifyNavigateToPage($"chat/{_currentChatSession.Id}");
            NotifyRefreshView();
        }
    }

    public void CloseChatSessionSettings()
    {
        NotifyNavigateToPage("/");
        NotifyRefreshView();
    }

    public async Task EditCurrentChatSession(ChatSessionSettings chatSessionSettings)
    {
        if (_chatSessions is null)
        {
            return;
        }

        if (_currentChatSession is null || _currentChatSessionConfiguration is null)
        {
            return;
        }
        
        _isChatSessionBeingEdited = true;
        
        ChatSessionConfigurationDto chatSessionConfigurationDto = new()
        {
            ChatbotName = chatSessionSettings.ChatbotName,
            TextGenerationChatbotModel = chatSessionSettings.TextGenerationChatbotModel,
            ImageGenerationChatbotModel = chatSessionSettings.ImageGenerationChatbotModel,
            Temperature = chatSessionSettings.Temperature,
            SystemInstruction = chatSessionSettings.SystemInstruction,
            TextStream = chatSessionSettings.TextStream
        };

        var title = chatSessionSettings.Title.Trim();
        if (title == string.Empty)
        {
            title = "Undefined";
        }
        else if (title.Length > 20)
        {
            title = title.Substring(0, 20);
        }

        ChatSessionEditDto chatSessionEditDto = new()
        {
            Title = title,
            ChatSessionConfigurationDto = chatSessionConfigurationDto
        };

        var editChatSessionResponse = await HttpClient.PutAsJsonAsync($"chat-sessions/{_currentChatSession.Id}/edit", chatSessionEditDto);

        if (!editChatSessionResponse.IsSuccessStatusCode)
        {
            _isChatSessionBeingEdited = false;
            var editChatSessionResponseContent = await editChatSessionResponse.Content.ReadAsStringAsync();
            HandleError($"Error occured while editing the chat session {chatSessionEditDto.Title}", 
                editChatSessionResponseContent);
            return;
        }
        
        _currentChatSession.Title = title;
        _currentChatSessionConfiguration.ChatbotName = chatSessionSettings.ChatbotName;
        _currentChatSessionConfiguration.TextGenerationChatbotModel = chatSessionSettings.TextGenerationChatbotModel;
        _currentChatSessionConfiguration.ImageGenerationChatbotModel = chatSessionSettings.ImageGenerationChatbotModel;
        _currentChatSessionConfiguration.Temperature = chatSessionSettings.Temperature;
        _currentChatSessionConfiguration.SystemInstruction = chatSessionSettings.SystemInstruction;
        _currentChatSessionConfiguration.TextStream = chatSessionSettings.TextStream;
        
        var chatbot = _settingsService.GetChatbotByName(chatSessionSettings.ChatbotName);
        if (chatbot is not null)
        {
            _currentChatSessionConfiguration.SupportBase64InputImageFormat = chatbot.SupportBase64ImageInputFormat;
            _currentChatSessionConfiguration.SupportUrlInputImageFormat = chatbot.SupportUrlImageInputFormat;
            _currentChatSessionConfiguration.SupportImageGeneration = chatbot.SupportImageGeneration;
        }
        
        _isChatSessionBeingEdited = false;
        NotifyNavigateToPage("/");
        NotifyRefreshView();
    }

    public async Task DeleteChatSession(ChatSessionDto chatSessionDto)
    {
        if (_chatSessions is null)
        {
            return;
        }
        
        if (!CanUserInteract())
        {
            return;
        }

        if (!IsChatSessionValid(chatSessionDto))
        {
            return;
        }

        _isChatSessionBeingDeleted = true;

        var deleteChatSessionResponse = await HttpClient.DeleteAsync($"chat-sessions/{chatSessionDto.Id}/delete");

        if (!deleteChatSessionResponse.IsSuccessStatusCode)
        {
            _isChatSessionBeingDeleted = false;
            var deleteChatSessionResponseContent = await deleteChatSessionResponse.Content.ReadAsStringAsync();
            HandleError("Error occured while deleting the chat session", deleteChatSessionResponseContent);
            return;
        }
        
        _chatSessions.Remove(chatSessionDto);
        _isChatSessionBeingDeleted = false;

        if (chatSessionDto == _currentChatSession)
        {
            DeselectCurrentChatSession();
            
            if (_chatSessions.Count > 0)
            {
                await SelectChatSession(_chatSessions.ElementAt(0));
            }
            else
            {
                NotifyNavigateToPage("/welcome");
            }
        }
        
        NotifyRefreshView();
    }

    public async Task LoadChatSessions()
    {
        if (_chatSessions is not null)
        {
            return;
        }

        _areChatSessionsLoadingOnStartup = true;
        
        var response = await HttpClient.GetAsync($"chat-sessions");
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _areChatSessionsLoadingOnStartup = false;
            HandleError("Error occured while loading the chat sessions", responseContent);
            return;
        }
        
        var chatSessionsDto = JsonSerializer.Deserialize<List<ChatSessionDto>>(responseContent) 
                              ?? new List<ChatSessionDto>();
        
         _chatSessions = chatSessionsDto;

         if (_chatSessions.Count > 0)
         {
             NotifyNavigateToPage("/");
         }
         else
         {
             NotifyNavigateToPage("/welcome");
         }
        
        _areChatSessionsLoadingOnStartup = false;
        
        NotifyRefreshView();
                
        if (_chatSessions is not null && _chatSessions.Count > 0)
        {
            await SelectChatSession(_chatSessions.ElementAt(0));
        }
        
        NotifyRefreshView();
    }
    
    public bool CanUserInteract()
    {
        return !_areChatSessionsLoadingOnStartup
               && !_isNewChatSessionBeingCreated
               && !_isChatSessionBeingSelected
               && !_isGeneratingResponse
               && !_isChatSessionBeingDeleted
               && !_isChatSessionBeingEdited;
    }

    private bool IsChatSessionValid(ChatSessionDto chatSessionDto)
    {
        return chatSessionDto.Id is not null
               && chatSessionDto.Title is not null
               && chatSessionDto.CreationDate is not null;
    }

    private bool IsMessageValid(MessageDto messageDto)
    {
        return messageDto.Text is not null
               && messageDto.Role is not null
               && messageDto.CreationDate is not null;
    }

    private async Task<MessageDto> SendUserMessage(string text, List<MediaDto>? fileInputs = null)
    {
        MessageDto userTextMessageDto = new MessageDto();
        userTextMessageDto.Text = text;
        userTextMessageDto.MediaDtos = fileInputs;
        
        var userMessageResponse = await HttpClient
            .PostAsJsonAsync($"chat-session/{_currentChatSession.Id}/new-user-message", 
                userTextMessageDto);
        var userMessageResponseContent = await userMessageResponse.Content.ReadAsStringAsync();

        if (!userMessageResponse.IsSuccessStatusCode)
        {
            return CreateNewErrorMessage("userMessageResponseContent");
        }
            
        var userMessage = JsonSerializer.Deserialize<MessageDto>(userMessageResponseContent);

        if (userMessage is null)
        {
            return CreateNewErrorMessage("The user message could not be deserialized");
        }

        return userMessage;
    }
    
    private async Task<MessageDto> GenerateAssistantTextMessage(ChatSessionInfoDto chatSessionInfoDto)
    {
        var assistantMessageResponse = await 
            HttpClient.PostAsJsonAsync(
                $"chat-session/{_currentChatSession.Id}/new-assistant-message", 
                chatSessionInfoDto);
        var assistantMessageResponseContent = await assistantMessageResponse.Content.ReadAsStringAsync();
            
        if (!assistantMessageResponse.IsSuccessStatusCode)
        {
            return CreateNewErrorMessage(assistantMessageResponseContent);
        }
        
        MessageDto? assistantMessageDto = JsonSerializer.Deserialize<MessageDto>(assistantMessageResponseContent);
        
        if (assistantMessageDto is null || !IsMessageValid(assistantMessageDto))
        {
            return CreateNewErrorMessage("The assistant message could not be deserialized");
        }

        return assistantMessageDto;
    }

    private async Task GenerateAssistantTextMessageWithStreamEnabled(ChatSessionInfoDto chatSessionInfoDto)
    {
        var assistantEmptyMessageResponse = await 
                HttpClient.PostAsJsonAsync(
                $"chat-session/{_currentChatSession.Id}/new-assistant-empty-message", 
                chatSessionInfoDto);
            var assistantEmptyMessageResponseContent = await assistantEmptyMessageResponse.Content.ReadAsStringAsync();
            
            if (!assistantEmptyMessageResponse.IsSuccessStatusCode)
            {
                _isGeneratingResponse = false;
                HandleError("Error occured while creating a new assistant message", assistantEmptyMessageResponseContent);
                return;
            }
            
            var assistantEmptyMessageDto = JsonSerializer.Deserialize<MessageDto>(assistantEmptyMessageResponseContent);

            if (assistantEmptyMessageDto is null || !IsMessageValid(assistantEmptyMessageDto))
            {
                _isGeneratingResponse = false;
                HandleError("Error occured while creating a new assistant message", 
                    "The assistant message could not be deserialized");
                return;
            }
            
            _currentChatSessionMessages.Add(assistantEmptyMessageDto);
            
            using var assistantStreamMessageRequest = new HttpRequestMessage(HttpMethod.Post, 
                $"chat-session/{_currentChatSession.Id}/generate-assistant-stream-message");
            assistantStreamMessageRequest.Content = JsonContent.Create(chatSessionInfoDto);
        
            using var assistantStreamMessageResponse = await HttpClient.SendAsync(assistantStreamMessageRequest, 
                HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            var assistantStreamMessageResponseContent = await assistantStreamMessageResponse.Content.ReadAsStreamAsync()
                .ConfigureAwait(false);
            
            if (!assistantStreamMessageResponse.IsSuccessStatusCode)
            {
                _isGeneratingResponse = false;
                HandleError("Error occured while generating a new assistant text stream message", 
                    assistantStreamMessageResponseContent.ToString() ?? "");
                return;
            }
            
            IAsyncEnumerable<string?> messageChunks =
                JsonSerializer.DeserializeAsyncEnumerable<string>(assistantStreamMessageResponseContent);
            
            _isGeneratingResponse = false;
            NotifyRefreshView();

            await foreach (var messageChunk in messageChunks)
            {
                if (messageChunk != string.Empty)
                {
                    assistantEmptyMessageDto.Text += messageChunk;
                    NotifyRefreshView();

                    await Task.Delay(chatSessionInfoDto.TextStreamDelay);
                }
            }
    }

    private async Task<MessageDto> GenerateAssistantImageMessage(ChatSessionInfoDto chatSessionInfoDto)
    {
        var assistantMessageResponse = await 
            HttpClient.PostAsJsonAsync(
                $"chat-session/{_currentChatSession.Id}/new-assistant-image-message", 
                chatSessionInfoDto);
        var assistantMessageResponseContent = await assistantMessageResponse.Content.ReadAsStringAsync();
            
        if (!assistantMessageResponse.IsSuccessStatusCode)
        {
            return CreateNewErrorMessage($"{assistantMessageResponseContent}");
        }
        
        MessageDto? assistantMessageDto = JsonSerializer.Deserialize<MessageDto>(assistantMessageResponseContent);
        
        if (assistantMessageDto is null || !IsMessageValid(assistantMessageDto))
        {
            return CreateNewErrorMessage("The assistant message could not be deserialized");
        }

        return assistantMessageDto;
    }

    private MessageDto CreateNewErrorMessage(string text)
    {
        return new MessageDto
        {
            Role = "assistant",
            Status = "error",
            Text = text
        };
    }

    private void HandleError(string errorTitle, string errorMessage)
    {
        if (_currentChatSessionMessages is null)
        {
            NotifyNavigateToErrorPage(errorTitle, errorMessage);
        }
        else
        {
            _currentChatSessionMessages.Add(
                CreateNewErrorMessage($"{errorTitle}: {errorMessage}"));
            NotifyRefreshView();
        }
    }
}