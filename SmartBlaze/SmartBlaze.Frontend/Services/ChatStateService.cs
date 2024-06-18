using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Controllers;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class ChatStateService
{
    private ChatSessionController _chatSessionController;
    
    private List<Chat>? _chats;
    private ChatSession? _currentChatSession;

    public ChatStateService(ChatSessionController chatSessionController)
    {
        _chatSessionController = chatSessionController;
        
        if (_chats is null)
        {
            LoadChats();
        }
    }

    public List<Chat>? Chats
    {
        get => _chats;
    }

    public ChatSession? CurrentChatSession
    {
        get => _currentChatSession;
    }
    
    public event Action? OnChange;
    
    public void SelectChat(Chat chat)
    {
        if (_chats is null)
        {
            return;
        }
        
        if (_currentChatSession is not null)
        {
            Chat? selectedChat = _chats.Find(c => c.Id == _currentChatSession.Id);

            if (selectedChat is not null)
            {
                selectedChat.Selected = false;
            }
        }
        
        Chat? newSelectedChat = _chats.Find(c => c.Id == chat.Id);

        if (newSelectedChat is not null)
        {
            newSelectedChat.Selected = true;
        }

        _currentChatSession = _chatSessionController.GetChatSessionById(chat.Id);
        
        NotifyStateChanged();
    }

    public async void SendUserMessage(string content)
    {
        content = content.Trim();

        if (content == string.Empty)
        {
            return;
        }

        if (_currentChatSession is not null)
        {
            Message userMessage = _chatSessionController.CreateNewUserMessage(content);
            _chatSessionController.AddNewMessageToChatSession(userMessage, _currentChatSession);
            NotifyStateChanged();

            Message? assistantMessage = await _chatSessionController.GenerateNewAssistantMessage(_currentChatSession);
            if (assistantMessage is not null)
            {
                _chatSessionController.AddNewMessageToChatSession(assistantMessage, _currentChatSession);
                NotifyStateChanged();
            }
            else
            {
                //done sth else
            }
        }
    }

    public async void CreateNewChatSession()
    {
        ChatSession chatSession = _chatSessionController.CreateNewChatSession("undefined");
        _chatSessionController.AddChatSession(chatSession);

        Chat chat = new Chat(chatSession.Id, chatSession.Title);
        if (_chats is null)
        {
            _chats = new List<Chat>();
        }
        _chats.Insert(0, chat);
        
        SelectChat(chat);

        if (_currentChatSession is not null)
        {
            Message systemMessage = _chatSessionController.CreateNewSystemMessage(
                "You are a helpful assistant. You can help me by answering my questions. " +
                "You can also ask me questions.");
            _chatSessionController.AddNewMessageToChatSession(systemMessage, _currentChatSession);
        }
        
        NotifyStateChanged();
    }

    private void LoadChats()
    {
        _chats = _chatSessionController.GetAllChatSessions()
            .Select(cs => new Chat(cs.Id, cs.Title)).ToList();
        
        if (_chats.Count > 0)
        {
            SelectChat(_chats.ElementAt(0));
        }
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}