using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Som_Models.DTOs;
using Som_Service.Interface;

namespace Som_Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IAiAssistantService _aiService;

        public AiController(IAiAssistantService aiService)
        {
            _aiService = aiService;
        }

        /// <summary>
        /// Ask Google Gemini AI to solve and explain a homework problem step-by-step
        /// </summary>
        [HttpPost("solve")]
        public async Task<IActionResult> SolveHomework([FromBody] AiSolveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.QuestionText) && string.IsNullOrWhiteSpace(request.ImageBase64))
            {
                return BadRequest(new { message = "Question text or an image of the homework problem is required." });
            }

            var response = await _aiService.SolveHomeworkWithAiAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
