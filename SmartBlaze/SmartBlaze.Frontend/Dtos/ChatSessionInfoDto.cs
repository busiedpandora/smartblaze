namespace SmartBlaze.Frontend.Dtos;

public class ChatSessionInfoDto
{
    public List<MessageDto>? Messages { get; set; }
    public string? Apihost { get; set; }
    public string? ApiKey { get; set; }
}