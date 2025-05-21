using Microsoft.AspNetCore.Mvc;
using WebShop.BLL.DTOs;
using WebShop.BLL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CommentDto>> AddComment([FromBody] CommentDto commentDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("ID не знайдено або не валідний."); 
            }
            var addedComment = await _commentService.AddCommentAsync(commentDto, userId);
            return CreatedAtAction(nameof(GetCommentsByProduct), new { productId = addedComment.ProductId }, addedComment);
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByProduct(int productId)
        {
            var comments = await _commentService.GetCommentsByProductAsync(productId);
            return Ok(comments);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] CommentDto commentDto)
        {
            await _commentService.UpdateCommentAsync(id, commentDto);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            await _commentService.DeleteCommentAsync(id);
            return NoContent();
        }
    }
}