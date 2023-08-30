using E2.Firebase;
using E2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        [HttpPost("send")]
        [AllowAnonymous]
        public IActionResult SendNotification([FromBody] NotificationModel model)
        {
            try
            {
                FirebaseCloudMessaging.SendNotification(model.DeviceToken, model.Title, model.Body);
                return Ok("Notification sent successfully.");
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
