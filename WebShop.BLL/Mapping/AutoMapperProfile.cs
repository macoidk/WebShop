using System.Runtime.CompilerServices;
using AutoMapper;
using WebShop.BLL.DTOs;
using WebShop.Models;


namespace WebShop.BLL.Mapping
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
            CreateMap<Order, OrderDto>();
            CreateMap<OrderDto, Order>();
            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<OrderItemDto, OrderItem>();
            CreateMap<Comment, CommentDto>();
            CreateMap<CommentDto, Comment>();
            CreateMap<Rating, RatingDto>();
            CreateMap<RatingDto, Rating>();
        }
    }
}