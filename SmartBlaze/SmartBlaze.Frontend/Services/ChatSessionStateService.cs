using System.Text.Json;
using SmartBlaze.Frontend.Dtos;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class ChatSessionStateService(IHttpClientFactory httpClientFactory) : AbstractService(httpClientFactory)
{
    private SettingsService _settingsService;
    private UserStateService _userStateService;
    
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


    public ChatSessionStateService(IHttpClientFactory httpClientFactory, SettingsService settingsService,
        UserStateService userStateService) 
        : this(httpClientFactory)
    {
        _settingsService = settingsService;
        _userStateService = userStateService;
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
    
    public event Action? ScrollToBottom;

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

        if (newSelectedChatSession is null || newSelectedChatSession.Id is null)
        {
            _isChatSessionBeingSelected = false;
            HandleError("Error occured while selecting the chat session", $"chat session with id {chatSession.Id} not found");
            return;
        }

        if (newSelectedChatSession.ChatSessionConfiguration is null)
        {
            var chatSessionConfiguration = await GetChatSessionConfiguration(newSelectedChatSession.Id,
                newSelectedChatSession.Title ?? "");

            if (chatSessionConfiguration is null)
            {
                return;
            }

            newSelectedChatSession.ChatSessionConfiguration = chatSessionConfiguration;
        }
        
        _currentChatSessionConfiguration = newSelectedChatSession.ChatSessionConfiguration;
        
        var chatbot = _settingsService.GetChatbot(_currentChatSessionConfiguration?.ChatbotName);
        _settingsService.ChatbotSelectedInCurrentChatSession = chatbot;
        SwitchToTextGeneration();

        if (newSelectedChatSession.Messages is null)
        {
            var messages = await GetChatSessionMessages(newSelectedChatSession.Id);

            if (messages is null)
            {
                return;
            }

            newSelectedChatSession.Messages = messages;
        }

        _currentChatSessionMessages = newSelectedChatSession.Messages;
        
        newSelectedChatSession.Selected = true;

        if (_currentChatSession is not null)
        {
            _currentChatSession.Selected = false;
        }

        _currentChatSession = newSelectedChatSession;
        
        NotifyNavigateToPage("/");

        _isChatSessionBeingSelected = false;
        
        NotifyRefreshView();
        
        NotifyScrollToBottom();
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

            _settingsService.ChatbotSelectedInCurrentChatSession = null;
            _settingsService.ChatbotModelSelectedInCurrentChatSession = null;
            
            NotifyRefreshView();
        }
    }

    public async Task RequestNewAssistantTextMessage(string text, List<MediaDto> fileInputs)
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

        var chatbotDefaultConfiguration =
            _settingsService.GetChatbotDefaultConfiguration(_currentChatSessionConfiguration.ChatbotName);

        if (chatbotDefaultConfiguration is null)
        {
            return;
        }

        var chatbotModel = _settingsService.GetTextGenerationChatbotModel(_currentChatSessionConfiguration.ChatbotName,
            _currentChatSessionConfiguration.TextGenerationChatbotModel);

        if (chatbotModel is null)
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
            LastUserMessage = userMessage,
            ChatbotName = _currentChatSessionConfiguration.ChatbotName,
            ChatbotModel = _currentChatSessionConfiguration.TextGenerationChatbotModel,
            ApiHost = chatbotDefaultConfiguration.ApiHost,
            ApiKey = chatbotDefaultConfiguration.ApiKey,
            SystemInstruction = _currentChatSessionConfiguration.SystemInstruction,
            Temperature = _currentChatSessionConfiguration.Temperature,
            TextStreamDelay = chatbotModel.TextStreamDelay
        };

        if (_currentChatSessionMessages.Count == 1)
        {
            await EntitleChatSessionFromUserMessage(chatSessionInfoDto);
        }
        
        if (chatbotModel.AcceptTextStream && _currentChatSessionConfiguration.TextStream)
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

    public async Task RequestNewAssistantImageMessage(string text)
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
        
        var chatbotDefaultConfiguration =
            _settingsService.GetChatbotDefaultConfiguration(_currentChatSessionConfiguration.ChatbotName);

        if (chatbotDefaultConfiguration is null)
        {
            return;
        }

        var chatbotModel = _settingsService.GetImageGenerationChatbotModel(_currentChatSessionConfiguration.ChatbotName,
            _currentChatSessionConfiguration.ImageGenerationChatbotModel);

        if (chatbotModel is null)
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
            ApiHost = chatbotDefaultConfiguration.ApiHost,
            ApiKey = chatbotDefaultConfiguration.ApiKey,
            ImageSize = _currentChatSessionConfiguration.ImageSize,
            ImagesToGenerate = _currentChatSessionConfiguration.ImagesToGenerate
        };
        
        if (_currentChatSessionMessages.Count == 1)
        {
            await EntitleChatSessionFromUserMessage(chatSessionInfoDto);
        }

        var assistantMessage = await GenerateAssistantImageMessage(chatSessionInfoDto);
        _currentChatSessionMessages.Add(assistantMessage);
        
        _isGeneratingResponse = false;
        
        NotifyRefreshView();
    }

    public async Task CreateNewChatSession()
    {
        if (!CanUserInteract())
        {
            return;
        }

        if (_settingsService.ChatbotDefaultConfigurationSelected is null 
            || _settingsService.ChatSessionDefaultConfiguration is null)
        {
            return;
        }

        if (_userStateService.UserLogged is null)
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
        chatSessionDto.Title = "Undefined";
        
        var newChatSessionResponse = await HttpClient.PostAsJsonAsync(
            $"chat-sessions/{_userStateService.UserLogged.Id}/new", chatSessionDto);
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
                "The chat session could not be deserialized");
            return;
        }
        
        ChatSessionConfigurationDto chatSessionConfigurationDto = new()
        {
            ChatbotName = _settingsService.ChatbotDefaultConfigurationSelected.ChatbotName,
            TextGenerationChatbotModel = _settingsService.ChatbotDefaultConfigurationSelected.TextGenerationChatbotModel,
            ImageGenerationChatbotModel = _settingsService.ChatbotDefaultConfigurationSelected.ImageGenerationChatbotModel,
            Temperature = _settingsService.ChatbotDefaultConfigurationSelected.Temperature,
            SystemInstruction = _settingsService.ChatSessionDefaultConfiguration.SystemInstruction,
            TextStream = _settingsService.ChatSessionDefaultConfiguration.TextStream,
            ImageSize = _settingsService.ChatSessionDefaultConfiguration.ImageSize,
            ImagesToGenerate = _settingsService.ChatSessionDefaultConfiguration.ImagesToGenerate
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
            TextStream = chatSessionSettings.TextStream,
            ImageSize = chatSessionSettings.ImageSize,
            ImagesToGenerate = chatSessionSettings.ImagesToGenerate
        };

        var title = chatSessionSettings.Title.Trim();
        if (title == string.Empty)
        {
            title = "Undefined";
        }
        else if (title.Length > 30)
        {
            title = title.Substring(0, 30);
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
        _currentChatSessionConfiguration.ImageSize = chatSessionSettings.ImageSize;
        _currentChatSessionConfiguration.ImagesToGenerate = chatSessionSettings.ImagesToGenerate;
        
        var chatbot = _settingsService.GetChatbot(_currentChatSessionConfiguration?.ChatbotName);
        _settingsService.ChatbotSelectedInCurrentChatSession = chatbot;
        SwitchToTextGeneration();
        
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
        }
        
        NotifyRefreshView();
    }

    public async Task LoadChatSessions()
    {
        if (_chatSessions is not null)
        {
            return;
        }

        if (_userStateService.UserLogged is null)
        {
            return;
        }

        _areChatSessionsLoadingOnStartup = true;
        NotifyRefreshView();
        
        var response = await HttpClient.GetAsync($"chat-sessions/{_userStateService.UserLogged.Id}");
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
         
        _areChatSessionsLoadingOnStartup = false;
        
        NotifyRefreshView();
                
        if (_chatSessions is not null && _chatSessions.Count > 0)
        {
            await SelectChatSession(_chatSessions.ElementAt(0));
        }
        
        NotifyRefreshView();
    }

    public void Logout()
    {
        if (_currentGenerationType != "text")
        {
            SwitchToTextGeneration();
        }
        
        DeselectCurrentChatSession();

        _chatSessions = null;
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

    public void SwitchToTextGeneration()
    {
        var chatbot = _settingsService.ChatbotSelectedInCurrentChatSession;

        var chatbotModel =
            _settingsService.GetTextGenerationChatbotModel(chatbot,
                _currentChatSessionConfiguration?.TextGenerationChatbotModel);
            
        _settingsService.ChatbotModelSelectedInCurrentChatSession = chatbotModel;
        
        _currentGenerationType = "text";
        NotifyRefreshView();
    }

    public void SwitchToImageGeneration()
    {
        var chatbot = _settingsService.ChatbotSelectedInCurrentChatSession;

        var chatbotModel =
            _settingsService.GetImageGenerationChatbotModel(chatbot,
                _currentChatSessionConfiguration?.ImageGenerationChatbotModel);
            
        _settingsService.ChatbotModelSelectedInCurrentChatSession = chatbotModel;
        
        _currentGenerationType = "image";
        NotifyRefreshView();
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
            .PostAsJsonAsync($"chat-session/{_currentChatSession?.Id}/new-user-message", 
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
                $"chat-session/{_currentChatSession?.Id}/new-assistant-message", 
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
                $"chat-session/{_currentChatSession?.Id}/new-assistant-empty-message", 
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
                $"chat-session/{_currentChatSession?.Id}/generate-assistant-stream-message");
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
                $"chat-session/{_currentChatSession?.Id}/new-assistant-image-message", 
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

    private async Task EntitleChatSessionFromUserMessage(ChatSessionInfoDto chatSessionInfoDto)
    {
        var assistantMessageResponse = await 
            HttpClient.PutAsJsonAsync(
                $"chat-sessions/{_currentChatSession?.Id}/entitle", 
                chatSessionInfoDto);
        var assistantMessageResponseContent = await assistantMessageResponse.Content.ReadAsStringAsync();

        if (assistantMessageResponse.IsSuccessStatusCode && _currentChatSession is not null)
        {
            _currentChatSession.Title = assistantMessageResponseContent;
            NotifyRefreshView();
        }
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

    private async Task<ChatSessionConfigurationDto?> GetChatSessionConfiguration(string chatSessionId, string chatSessionTitle)
    {
        var chatSessionConfigurationResponse = await HttpClient.GetAsync($"configuration/chat-session/{chatSessionId}");
        var chatSessionConfigurationResponseContent =
            await chatSessionConfigurationResponse.Content.ReadAsStringAsync();

        if (!chatSessionConfigurationResponse.IsSuccessStatusCode)
        {
            _isChatSessionBeingSelected = false;
            HandleError($"Error occured while selecting the chat session {chatSessionTitle}", 
                chatSessionConfigurationResponseContent);
            return null;
        }

        var chatSessionConfiguration =
            JsonSerializer.Deserialize<ChatSessionConfigurationDto>(chatSessionConfigurationResponseContent);

        return chatSessionConfiguration;
    }

    private async Task<List<MessageDto>?> GetChatSessionMessages(string chatSessionId)
    {
        var response = await HttpClient.GetAsync($"chat-session/{chatSessionId}/messages");
        var responseContent = await response.Content.ReadAsStringAsync();
            
        if (!response.IsSuccessStatusCode)
        {
            _isChatSessionBeingSelected = false;
            HandleError("Error occured while selecting the chat session", responseContent);
            return null;
        }

        var messages = new List<MessageDto>();

        if (responseContent != string.Empty)
        {
            messages = JsonSerializer.Deserialize<List<MessageDto>>(responseContent) ?? new List<MessageDto>();
        }

        return messages;
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

    private void NotifyScrollToBottom() => ScrollToBottom?.Invoke();
}