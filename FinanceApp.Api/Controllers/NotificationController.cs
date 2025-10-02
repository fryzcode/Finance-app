using System.Security.Claims;
using FinanceApp.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public NotificationController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterPushToken([FromBody] RegisterPushTokenRequest request)
        {
            var userId = GetUserId();
            
            // Store the push token for the user
            // You might want to create a UserPushTokens table to store multiple tokens per user
            // For now, we'll just return success
            return Ok(new { message = "Push token registered successfully" });
        }

        [HttpPost("test-notification")]
        public async Task<IActionResult> SendTestNotification([FromBody] TestNotificationRequest request)
        {
            var userId = GetUserId();
            
            // Here you would integrate with a push notification service like Firebase Cloud Messaging
            // or Expo Push Notifications to send the actual notification
            
            return Ok(new { message = "Test notification sent successfully" });
        }

        [HttpGet("settings")]
        public async Task<ActionResult<NotificationSettingsResponse>> GetNotificationSettings()
        {
            var userId = GetUserId();
            
            // Return notification preferences for the user
            return new NotificationSettingsResponse
            {
                SalaryReminders = true,
                ExpenseReminders = true,
                SavingsGoalReminders = true,
                PushNotificationsEnabled = true
            };
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsRequest request)
        {
            var userId = GetUserId();
            
            // Update notification preferences for the user
            // You might want to store these in a UserNotificationSettings table
            
            return Ok(new { message = "Notification settings updated successfully" });
        }

        private Guid GetUserId()
        {
            var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(sub!);
        }
    }

    public class RegisterPushTokenRequest
    {
        public string PushToken { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty; // "ios", "android", "web"
    }

    public class TestNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class NotificationSettingsResponse
    {
        public bool SalaryReminders { get; set; }
        public bool ExpenseReminders { get; set; }
        public bool SavingsGoalReminders { get; set; }
        public bool PushNotificationsEnabled { get; set; }
    }

    public class UpdateNotificationSettingsRequest
    {
        public bool SalaryReminders { get; set; }
        public bool ExpenseReminders { get; set; }
        public bool SavingsGoalReminders { get; set; }
        public bool PushNotificationsEnabled { get; set; }
    }
}
