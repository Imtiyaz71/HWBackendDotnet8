using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Som_Models.Models;
using Som_Service.Interface;
using System.Security.Claims;

namespace Som_Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProblemFeedController : ControllerBase
    {
        private readonly IProblemFeedService _feedService;
        private readonly IWebHostEnvironment _env;

        public ProblemFeedController(IProblemFeedService feedService, IWebHostEnvironment env)
        {
            _feedService = feedService;
            _env = env;
        }

        public class CreateProblemDto
        {
            public string SubjectName { get; set; } = string.Empty;
            public string ClassName { get; set; } = string.Empty;
            public string CurriculumType { get; set; } = "General";
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public IFormFile? ImageFile { get; set; }
        }

        public class AddReplyDto
        {
            public string ReplyText { get; set; } = string.Empty;
            public IFormFile? ImageFile { get; set; }
            public IFormFile? AudioFile { get; set; }
        }

        /// <summary>
        /// Student posts a homework problem with optional picture
        /// </summary>
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProblem([FromForm] CreateProblemDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int studentId))
            {
                return Unauthorized(new { message = "Invalid user identification." });
            }

            var studentName = User.FindFirstValue(ClaimTypes.Name) ?? "Student";
            var studentPhone = User.FindFirstValue(ClaimTypes.MobilePhone) ?? "";

            string? imageUrl = null;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", "Problems");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var ext = Path.GetExtension(dto.ImageFile.FileName);
                var fileName = $"problem_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                imageUrl = $"/Uploads/Problems/{fileName}";
            }

            var problem = await _feedService.CreateProblemAsync(
                studentId,
                studentName,
                studentPhone,
                "School",
                dto.ClassName,
                dto.CurriculumType,
                dto.SubjectName,
                dto.Title,
                dto.Description,
                imageUrl);

            return Ok(problem);
        }

        /// <summary>
        /// Newsfeed for Teachers or Students
        /// </summary>
        [Authorize]
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] string? subjectName = null, [FromQuery] string? className = null)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);

            if (role == "Student")
            {
                var list = await _feedService.GetStudentFeedAsync(userId);
                return Ok(list);
            }
            else
            {
                var list = await _feedService.GetTeacherFeedAsync(userId, subjectName, className);
                return Ok(list);
            }
        }

        /// <summary>
        /// Get a problem by ID with full replies and like state
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProblemDetails(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int currentUserId);

            var problem = await _feedService.GetProblemByIdAsync(id, currentUserId);
            if (problem == null)
            {
                return NotFound(new { message = "Homework problem not found." });
            }

            return Ok(problem);
        }

        /// <summary>
        /// Teacher or Student replies/comments on a homework problem
        /// </summary>
        [Authorize]
        [HttpPost("{id}/reply")]
        public async Task<IActionResult> AddReply(int id, [FromForm] AddReplyDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "User";
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";

            string? imageUrl = null;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", "Replies");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var ext = Path.GetExtension(dto.ImageFile.FileName);
                var fileName = $"reply_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                imageUrl = $"/Uploads/Replies/{fileName}";
            }

            string? audioUrl = null;
            if (dto.AudioFile != null && dto.AudioFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", "Audio");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var ext = Path.GetExtension(dto.AudioFile.FileName);
                if (string.IsNullOrEmpty(ext)) ext = ".m4a";
                var fileName = $"voice_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.AudioFile.CopyToAsync(stream);
                }

                audioUrl = $"/Uploads/Audio/{fileName}";
            }

            var reply = await _feedService.AddReplyAsync(id, userId, userName, userRole, dto.ReplyText, imageUrl, audioUrl);
            return Ok(reply);
        }

        /// <summary>
        /// Like or Unlike a reply
        /// </summary>
        [Authorize]
        [HttpPost("reply/{id}/like")]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var (isLiked, totalLikes) = await _feedService.ToggleLikeReplyAsync(id, userId);
            return Ok(new
            {
                replyId = id,
                isLiked,
                totalLikes
            });
        }
    }
}
