using E2.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using E2.Models;
using Microsoft.AspNetCore.Authorization;

namespace E2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PasswordController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PasswordController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("SendCode")]
        public IActionResult RequestResetPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }
            Random random = new Random();
            int randomNumber = random.Next(10000, 99999);
            DateTime expirationDate = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour

            var resetToken = new PassowrdReset
            {
                UserId = user.UserId,
                Code = randomNumber,
                ExpirationDate = expirationDate
            };

            _context.PassowrdReset.Add(resetToken);
            _context.SaveChanges();

            string body = $"code to reset you password {randomNumber}";

            SendResetEmail(user.Email!, body);

            return Ok();
        }

        [HttpPost("VerifyResetToken")]
        public IActionResult VerifyResetToken(int code)
        {
            var resetToken = _context.PassowrdReset.FirstOrDefault(t => t.Code == code && t.ExpirationDate > DateTime.UtcNow);

            if (resetToken == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            return Ok();
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(int code, string newPassword)
        {
            var resetToken = _context.PassowrdReset.FirstOrDefault(t => t.Code == code && t.ExpirationDate > DateTime.UtcNow);

            if (resetToken == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == resetToken.UserId);

            if (user == null)
            {
                return NotFound();
            }
            string HashPassord = BCrypt.Net.BCrypt.HashPassword(newPassword);
            // Update user's password
            user.HashPassword = HashPassord;
            _context.SaveChanges();

            // Delete/reset the used token
            _context.PassowrdReset.Remove(resetToken);
            _context.SaveChanges();

            return Ok("Password reset successfully.");
        }

        private void SendResetEmail(string toEmail, string body)
        {
            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential("ammar.zenalden1@gmail.com", "cllgthvzuwzcvvtk");
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                var message = new MailMessage("your@email.com", toEmail, "Password Reset", body);
                smtpClient.Send(message);
            }
        }
    }
}

