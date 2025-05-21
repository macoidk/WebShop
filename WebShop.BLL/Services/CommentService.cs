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

        public async Task<CommentDto> AddCommentAsync(CommentDto commentDto, int userId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(commentDto.ProductId);
            if (product == null)
                throw new NotFoundException("Продукт не знайдено.");
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.Role == Models.UserRole.UnregisteredUser)
                throw new UnauthorizedException("Тільки зареєстровані користувачі можуть залишати коментарі.");
            var comment = _mapper.Map<Comment>(commentDto);
            comment.Date = DateTime.UtcNow;
            comment.UserId = user.Id;

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveAsync();

            var resultDto = _mapper.Map<CommentDto>(comment);
            resultDto.Username = user.Username;
            return resultDto;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByProductAsync(int productId)
        {
            var comments = await _unitOfWork.Comments.GetCommentsByProductAsync(productId);
            var commentDtos = new List<CommentDto>();
            foreach (var comment in comments)
            {
                var commentDto = _mapper.Map<CommentDto>(comment);
                var user = await _unitOfWork.Users.GetByIdAsync(comment.UserId);
                commentDto.Username = user?.Username ?? "Невідомий користувач";
                commentDtos.Add(commentDto);
            }
            return commentDtos;
        }

        public async Task UpdateCommentAsync(int id, CommentDto commentDto)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id);
            if (comment == null)
            {
                return;
            }
            _mapper.Map(commentDto, comment);
            await _unitOfWork.Comments.UpdateAsync(comment);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id);
            if (comment == null)
            {
                return;
            }
            await _unitOfWork.Comments.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
    }
}