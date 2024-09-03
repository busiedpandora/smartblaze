namespace SmartBlaze.Backend.Dtos;

public class AssistantMessageInfoDto
{
    public string? Text { get; set; }
    public List<MediaDto>? MediaDtos { get; set; }
    public required string Status { get; set; }
}