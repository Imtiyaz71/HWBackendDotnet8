using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Som_Service.Interface;
using System.Security.Claims;

namespace Som_Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IUserNotificationService _notificationService;

        public NotificationController(IUserNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var list = await _notificationService.GetNotificationsAsync(userId);
            return Ok(list);
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var success = await _notificationService.MarkNotificationAsReadAsync(id);
            return Ok(new { success });
        }
    }
}
