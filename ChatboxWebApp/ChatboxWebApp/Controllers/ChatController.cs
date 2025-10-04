using Microsoft.AspNetCore.Mvc;
using ChatboxWebApp.Models;
using ChatboxWebApp.Services;

namespace ChatboxWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly ChatGptService _chatService;

        public ChatController(ILogger<ChatController> logger, ChatGptService chatService)
        {
            _logger = logger;
            _chatService = chatService;
        }

        [HttpPost("ask")]
        public async Task<ActionResult<ChatGptResponse>> AskQuestion([FromBody] AskQuestionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Question))
                {
                    return BadRequest(new { error = "Question cannot be empty." });
                }

                var response = await _chatService.AskQuestionAsync(request.Question);
                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Maximum questions count"))
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing question");
                return StatusCode(500, new { error = "An error occurred while processing your question." });
            }
        }

        [HttpPost("ask-multiple")]
        public async Task<ActionResult<ChatGptResponse>> AskMultipleQuestions([FromBody] AskMultipleQuestionsRequest request)
        {
            try
            {
                if (request.Questions == null || request.Questions.Count == 0)
                {
                    return BadRequest(new { error = "Questions list cannot be empty." });
                }

                var response = await _chatService.AskQuestionsAsync(request.Questions);
                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Maximum questions count"))
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing questions");
                return StatusCode(500, new { error = "An error occurred while processing your questions." });
            }
        }

        [HttpPost("new-conversation")]
        public ActionResult<ConversationInfoResponse> StartNewConversation()
        {
            try
            {
                var conversationId = _chatService.StartNewConversation();
                return Ok(new ConversationInfoResponse 
                { 
                    ConversationId = conversationId,
                    Message = "New conversation started."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting new conversation");
                return StatusCode(500, new { error = "An error occurred while starting a new conversation." });
            }
        }

        [HttpPost("reset")]
        public ActionResult<ConversationInfoResponse> ResetConversation()
        {
            try
            {
                _chatService.ResetConversation();
                var conversationId = _chatService.GetCurrentConversationId();
                return Ok(new ConversationInfoResponse 
                { 
                    ConversationId = conversationId,
                    Message = "Conversation reset."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting conversation");
                return StatusCode(500, new { error = "An error occurred while resetting the conversation." });
            }
        }

        [HttpGet("conversation-info")]
        public ActionResult<ConversationInfoResponse> GetConversationInfo()
        {
            try
            {
                var conversationId = _chatService.GetCurrentConversationId();
                var questionCount = _chatService.GetQuestionCount();
                var maxQuestions = _chatService.GetMaxQuestionsCount();
                
                return Ok(new ConversationInfoResponse 
                { 
                    ConversationId = conversationId,
                    QuestionCount = questionCount,
                    MaxQuestions = maxQuestions,
                    Message = $"Questions: {questionCount}/{maxQuestions}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation info");
                return StatusCode(500, new { error = "An error occurred while getting conversation info." });
            }
        }

        [HttpPost("continue")]
        public async Task<ActionResult<ConversationInfoResponse>> ContinueConversation([FromBody] ContinueConversationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ConversationId))
                {
                    return BadRequest(new { error = "Conversation ID cannot be empty." });
                }

                await _chatService.ContinueConversationAsync(request.ConversationId);
                return Ok(new ConversationInfoResponse 
                { 
                    ConversationId = request.ConversationId,
                    Message = "Conversation loaded."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error continuing conversation");
                return StatusCode(500, new { error = "An error occurred while loading the conversation." });
            }
        }
    }

    public class AskQuestionRequest
    {
        public string Question { get; set; } = string.Empty;
    }

    public class AskMultipleQuestionsRequest
    {
        public List<string> Questions { get; set; } = new List<string>();
    }

    public class ContinueConversationRequest
    {
        public string ConversationId { get; set; } = string.Empty;
    }

    public class ConversationInfoResponse
    {
        public string ConversationId { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public int MaxQuestions { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
