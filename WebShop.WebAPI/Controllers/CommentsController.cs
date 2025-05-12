using Microsoft.AspNetCore.Mvc;
using WebShop.BLL.DTOs;
using WebShop.BLL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
            var addedComment = await _commentService.AddCommentAsync(commentDto);
            return CreatedAtAction(nameof(GetCommentsByProduct), new { productId = addedComment.ProductId }, addedComment);
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByProduct(int productId)
        {
            var comments = await _commentService.GetCommentsByProductAsync(productId);
            return Ok(comments);
        }
    }
}