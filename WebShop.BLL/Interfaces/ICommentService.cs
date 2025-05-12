using System.Collections.Generic;
using System.Threading.Tasks;
using WebShop.BLL.DTOs;

namespace WebShop.BLL.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto> AddCommentAsync(CommentDto commentDto);
        Task<IEnumerable<CommentDto>> GetCommentsByProductAsync(int productId);
    }
}