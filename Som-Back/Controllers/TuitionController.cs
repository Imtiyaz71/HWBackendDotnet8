using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Som_Models.DTOs;
using Som_Service.Interface;
using System.Security.Claims;

namespace Som_Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TuitionController : ControllerBase
    {
        private readonly ITuitionService _tuitionService;

        public TuitionController(ITuitionService tuitionService)
        {
            _tuitionService = tuitionService;
        }

        /// <summary>
        /// Student or Guardian posts a new tuition requirement
        /// </summary>
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromBody] CreateTuitionPostDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var name = User.FindFirstValue(ClaimTypes.Name) ?? "Guardian";
            var phone = User.FindFirstValue(ClaimTypes.MobilePhone) ?? "";

            if (string.IsNullOrWhiteSpace(dto.ClassName) || string.IsNullOrWhiteSpace(dto.Subjects) || string.IsNullOrWhiteSpace(dto.Location))
            {
                return BadRequest(new { message = "Class, subjects, and location are required." });
            }

            var post = await _tuitionService.CreateTuitionPostAsync(userId, name, phone, dto);
            return Ok(post);
        }

        /// <summary>
        /// Browse open tuition posts (Teachers & Students)
        /// </summary>
        [Authorize]
        [HttpGet("feed")]
        public async Task<IActionResult> GetTuitionFeed(
            [FromQuery] string? className = null,
            [FromQuery] string? location = null,
            [FromQuery] string? curriculum = null)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);

            var list = await _tuitionService.GetTuitionPostsAsync(userId, className, location, curriculum);
            return Ok(list);
        }

        /// <summary>
        /// Teacher applies for a tuition post
        /// </summary>
        [Authorize(Policy = "TeacherOnly")]
        [HttpPost("{id}/apply")]
        public async Task<IActionResult> ApplyForTuition(int id, [FromBody] ApplyTuitionDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int teacherId))
            {
                return Unauthorized();
            }

            var teacherName = User.FindFirstValue(ClaimTypes.Name) ?? "Teacher";
            var teacherPhone = User.FindFirstValue(ClaimTypes.MobilePhone) ?? "";

            try
            {
                var app = await _tuitionService.ApplyForTuitionAsync(id, teacherId, teacherName, teacherPhone, dto);
                return Ok(app);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Poster views applications for their tuition
        /// </summary>
        [Authorize]
        [HttpGet("{id}/applications")]
        public async Task<IActionResult> GetApplications(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var list = await _tuitionService.GetPostApplicationsAsync(id, userId);
            return Ok(list);
        }

        /// <summary>
        /// Poster updates application status (e.g. Accepted, Rejected)
        /// </summary>
        [Authorize]
        [HttpPost("application/{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var success = await _tuitionService.UpdateApplicationStatusAsync(id, userId, dto.Status);
            if (!success)
            {
                return BadRequest(new { message = "Unable to update status." });
            }

            return Ok(new { success = true, status = dto.Status });
        }

        public class UpdateStatusDto
        {
            public string Status { get; set; } = "Accepted";
        }
    }
}
