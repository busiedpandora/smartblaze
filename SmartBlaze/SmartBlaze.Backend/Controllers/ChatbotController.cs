using Microsoft.AspNetCore.Mvc;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Services;

namespace SmartBlaze.Backend.Controllers;

[ApiController]
[Route("chatbot")]
public class ChatbotController : ControllerBase
{
    private ChatbotService _chatbotService;


    public ChatbotController(ChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpGet]
    public ActionResult<List<ChatbotDto>> GetAllChatbotsAndModels()
    {
        var chatbots = _chatbotService.GetAllChatbots();

        var chatbotDtos = new List<ChatbotDto>();
        
        foreach (var chatbot in chatbots)
        {
            var textGenerationModels = chatbot.TextGenerationChatbotModels
                .Select(tgm => new ChatbotModelDto
                {
                    Name = tgm.Name,
                    Type = "textGeneration",
                    AcceptSystemInstruction = tgm.AcceptSystemInstruction,
                    AcceptTemperature = tgm.AcceptTemperature,
                    MinTemperature = tgm.MinTemperature,
                    MaxTemperature = tgm.MaxTemperature,
                    AcceptBase64ImageInput = tgm.AcceptBase64ImageInput,
                    AcceptUrlImageInput = tgm.AcceptUrlImageInput,
                    AcceptTextStream = tgm.AcceptTextStream,
                    TextStreamDelay = tgm.TextStreamDelay,
                    AcceptImageVision = tgm.AcceptImageVision
                })
                .ToList();
            
            var imageGenerationModels = chatbot.ImageGenerationChatbotModels
                .Select(igm => new ChatbotModelDto
                {
                    Name = igm.Name,
                    Type = "imageGeneration",
                    AcceptBase64ImageInput = igm.AcceptBase64ImageInput,
                    AcceptUrlImageInput = igm.AcceptUrlImageInput,
                    AcceptImageSize = igm.AcceptImageSize,
                    ImageSizeSupport = igm.ImageSizeSupport,
                    AcceptMultipleImagesGenerationAtOnce = igm.AcceptMultipleImagesGenerationAtOnce,
                    MaxNumberOfGeneratedImagesAtOnce = igm.MaxNumberOfGeneratedImagesAtOnce
                })
                .ToList();
            
            ChatbotDto chatbotDto = new()
            {
                Name = chatbot.Name,
                TextGenerationChatbotModels = textGenerationModels,
                ImageGenerationChatbotModels = imageGenerationModels
            };
            
            chatbotDtos.Add(chatbotDto);
        }

        return Ok(chatbotDtos);
    }
}