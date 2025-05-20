using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using WebShop.BLL.DTOs;
using WebShop.BLL.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WebShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UsersController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        
        [HttpPost("create-user-with-role")]
        [Authorize(Roles = "Administrator")] 
        public async Task<IActionResult> CreateUserWithRole([FromBody] UserDto userDto, UserRole role)
        {
            var result = await _userService.CreateUserWithRoleAsync(userDto, userDto.Password, role);
            return Ok(result);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("role/{role}")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(string role)
        {
            var users = await _userService.GetUsersByRoleAsync((UserRole)Enum.Parse(typeof(UserRole), role));
            return Ok(users);
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser([FromBody] UserDto userDto)
        {
            await _userService.RegisterUserAsync(userDto, userDto.Password);
            return CreatedAtAction(nameof(GetUserById), new { id = userDto.Id }, userDto);
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser([FromBody] LoginModel loginModel)
        {
            var user = await _userService.LoginUserAsync(loginModel.Username, loginModel.Password);
            if (user == null)
                return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(new
            {
                User = user,
                Token = tokenString
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUserProfile(int id, [FromBody] UserDto userDto)
        {
            if (id != userDto.Id)
                return BadRequest();
            await _userService.UpdateUserProfileAsync(userDto);
            return NoContent();
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}