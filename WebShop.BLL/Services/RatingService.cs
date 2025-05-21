namespace WebShop.BLL.Services
{
    using AutoMapper;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using WebShop.Abstractions.UnitOfWork;
    using WebShop.BLL.DTOs;
    using WebShop.BLL.Exceptions;
    using WebShop.BLL.Interfaces;
    using WebShop.Models;

    public class RatingService : IRatingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RatingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<RatingDto> AddRatingAsync(RatingDto ratingDto)
        {
            if (ratingDto.Value < 1 || ratingDto.Value > 5)
                throw new ValidationException("Rating must be between 1 and 5.");
            var product = await _unitOfWork.Products.GetByIdAsync(ratingDto.ProductId);
            if (product == null)
                throw new NotFoundException("Product not found.");
            var user = await _unitOfWork.Users.GetByIdAsync(ratingDto.UserId);
            if (user == null || user.Role == Models.UserRole.UnregisteredUser)
                throw new UnauthorizedException("Only registered users can rate.");
            var rating = _mapper.Map<Rating>(ratingDto);
            await _unitOfWork.Ratings.AddAsync(rating);
            await _unitOfWork.SaveAsync();
            return ratingDto;
        }

        public async Task<IEnumerable<RatingDto>> GetRatingsByProductAsync(int productId)
        {
            var ratings = await _unitOfWork.Ratings.GetRatingsByProductAsync(productId);
            return _mapper.Map<IEnumerable<RatingDto>>(ratings);
        }

        public async Task UpdateRatingAsync(int id, RatingDto ratingDto)
        {
            var rating = await _unitOfWork.Ratings.GetByIdAsync(id);
            if (rating == null)
            {
                return;
            }
            _mapper.Map(ratingDto, rating);
            await _unitOfWork.Ratings.UpdateAsync(rating);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteRatingAsync(int id)
        {
            var rating = await _unitOfWork.Ratings.GetByIdAsync(id);
            if (rating == null)
            {
                return;
            }
            await _unitOfWork.Ratings.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
    }
}