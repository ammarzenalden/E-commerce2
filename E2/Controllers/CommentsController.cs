using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E2.Data;
using E2.Models;
using Microsoft.AspNetCore.Authorization;
using E2.DTO;
using System.Security.Claims;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using E2.Firebase;

namespace E2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }
        private int GetUserId()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            return int.Parse(userId!);
        }
        // GET: api/Comments
        [HttpGet("GetAllProductComments/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments(int id)
        {
            var comments = await _context.Comments.Where(x => x.ProductId == id).ToListAsync();
            return Ok(new
            {
                success = true,
                data = comments
            });
        }

        // GET: api/Comments/5
        [HttpGet("GetComment/{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound(new
                {
                    success = false,
                    messaga = "there is no comment with this id"
                });
            }

            return Ok(new
            {
                success = true,
                data = comment
            });
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("EditComment/{id}")]
        public async Task<IActionResult> PutComment(int id, CommentDto commentDto)
        {
            var comment = await _context.Comments.FindAsync(id);
            var user = GetUserId();
            if (comment is null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "there is no comment by this Id"
                });
            }
            if(comment.UserId == user)
            {
                comment.Text = commentDto.Text;
                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    data = comment
                });
            }
            else
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "you are not the owner of this comment"
                });
            }

        }

        // POST: api/Comments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("AddComment")]
        public async Task<ActionResult<Comment>> PostComment(CommentDto commentDto)
        {
            var product = _context.Products.Find(commentDto.ProductId);
            if(product is null)
            {
                return BadRequest(new
                {
                    success=false,
                    message="product not found"
                });
            }
            Comment comment = new()
            {
                UserId = GetUserId(),
                ProductId = commentDto.ProductId,
                Text = commentDto.Text
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            var deviceToken = _context.DeviceTokens.Where(p => p.UserId == product.UserId).ToList();
            //if (deviceToken != null && deviceToken.Count > 0)
            //{
            //    foreach (var device in deviceToken)
            //    {
            //        string token = device.Token;

            //        FirebaseCloudMessaging.SendNotification(token, "New comment", $"A new product with the name '{product.Name}' has a new comment.");
            //    }
            //}
            return Ok(new
            {
                success = true,
                data = comment
            });
            
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            int user = GetUserId();
            if (comment is null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "there is no comment by this Id"
                });
            }
            if (comment.UserId == user)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    success = true
                });
            }
            else
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "you are not the owner of the comment"
                });
            }
        }

    }
}
