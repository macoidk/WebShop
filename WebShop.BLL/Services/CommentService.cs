using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebShop.Abstractions.UnitOfWork;
using WebShop.BLL.DTOs;
using WebShop.BLL.Exceptions;
using WebShop.BLL.Interfaces;
using WebShop.Models;

namespace WebShop.BLL.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CommentDto> AddCommentAsync(CommentDto commentDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(commentDto.ProductId);
            if (product == null)
                throw new NotFoundException("Product not found.");
            var user = await _unitOfWork.Users.GetByIdAsync(commentDto.UserId);
            if (user == null || user.Role == Models.UserRole.UnregisteredUser)
                throw new UnauthorizedException("Only registered users can comment.");
            var comment = _mapper.Map<Comment>(commentDto);
            comment.Date = DateTime.UtcNow;
            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveAsync();
            return commentDto;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByProductAsync(int productId)
        {
            var comments = await _unitOfWork.Comments.GetCommentsByProductAsync(productId);
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }
    }
}