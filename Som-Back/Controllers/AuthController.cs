using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Som_Models.DTOs;
using Som_Service.Interface;
using System.Security.Claims;

namespace Som_Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IOpenAuthService _authService;

        public AuthController(IOpenAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Self-Registration for Students
        /// Default password is 123 if not provided.
        /// </summary>
        [HttpPost("register-student")]
        public async Task<IActionResult> RegisterStudent([FromBody] StudentRegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.StudentName) || string.IsNullOrWhiteSpace(dto.GuardianPhone))
            {
                return BadRequest(new { message = "Student name and guardian phone number are required." });
            }

            var response = await _authService.RegisterStudentAsync(dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Self-Registration for Teachers with Specialist Subjects
        /// Default password is 123 if not provided.
        /// </summary>
        [HttpPost("register-teacher")]
        public async Task<IActionResult> RegisterTeacher([FromBody] TeacherRegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TeacherName) || string.IsNullOrWhiteSpace(dto.Phone))
            {
                return BadRequest(new { message = "Teacher name and phone number are required." });
            }

            var response = await _authService.RegisterTeacherAsync(dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Unified Login using Phone OR Email and Password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Identifier) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Phone/Email and password are required." });
            }

            var response = await _authService.LoginUserAsync(dto);
            if (!response.Success)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Get currently logged in profile
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var phone = User.FindFirstValue(ClaimTypes.MobilePhone);
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new
            {
                userId,
                name,
                phone,
                role
            });
        }

        /// <summary>
        /// Change User Password
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var result = await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }
    }
}
