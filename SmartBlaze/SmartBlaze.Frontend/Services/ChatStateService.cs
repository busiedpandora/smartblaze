using SmartBlaze.Backend.Models;
using SmartBlaze.Backend.Services;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class ChatStateService
{
    private ChatSessionService _chatSessionService;
    
    private List<Chat>? _chats;
    private ChatSession? _currentChatSession;
    

    public ChatStateService(ChatSessionService chatSessionService)
    {
        _chatSessionService = chatSessionService;

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

        _currentChatSession = _chatSessionService.GetChatSessionById(chat.Id);
    }

    private void LoadChats()
    {
        _chats = _chatSessionService.GetAllChatSessions().Select(cs => new Chat(cs.Id, cs.Title)).ToList();
        
        if (_chats.Count > 0)
        {
            SelectChat(_chats.ElementAt(0));
        }
    }
}