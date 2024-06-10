using Microsoft.AspNetCore.Components;
using SmartBlaze.Backend.Services;
using SmartBlaze.Frontend.Models;

namespace SmartBlaze.Frontend.Services;

public class ChatStateService
{
    private ChatSessionService _chatSessionService;
    
    private List<Chat>? _chats;
    private Chat? _selectedChat;
    

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

    public Chat? SelectedChat
    {
        get => _selectedChat;
    }

    public void SelectChat(Chat chat)
    {
        if (_selectedChat is not null)
        {
            _selectedChat.Selected = false;
        }

        _selectedChat = chat;
        _selectedChat.Selected = true;
    }

    private void LoadChats()
    {
        _chats = _chatSessionService.GetAllChats().Select(cs => new Chat(cs.Id, cs.Title)).ToList();
        
        if (_chats.Count > 0)
        {
            SelectChat(_chats.ElementAt(0));
        }
    }
}