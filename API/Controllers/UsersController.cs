using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _service;

        public UsersController(IUsersService usersService)
        {
            _service = usersService;
        }

        // GET: api/<UsersController>
        [HttpGet]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<ActionResult<IEnumerable<UserDto>>> Get([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            if (page < 1 || limit < 1)
                return BadRequest("Page and limit must be greater than 0.");

            var (users, totalUsers) = await _service.GetPaginatedAsync(page, limit);

            var response = new
            {
                TotalItems = totalUsers,
                TotalPages = (int)Math.Ceiling((double)totalUsers / limit),
                CurrentPage = page,
                PageSize = limit,
                Users = users
            };

            return Ok(response);
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<ActionResult<UserDto>> Get(int id)
        {
            var user = await _service.GetByIdAsync(id);
            return Ok(user);
        }

        // POST api/<UsersController>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Post([FromBody] ManipulateUserDto user)
        {
            int userId = await _service.AddAsync(user);
            return Created($"User created with id:{userId}", null);
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] ManipulateUserDto userDto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != id && userRole != "Admin" && userRole != "Moderator")
                return Forbid();

            userDto.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.PasswordHash);//simplification of models, passowrd is actually hashed here
            await _service.UpdateAsync(id, userDto);
            return NoContent();
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]        
        public async Task<ActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != id && userRole != "Admin")
                return Forbid();

            await _service.DeleteAsync(id);
            return NoContent();
        }

        // POST api/<UsersController>/5/address
        [HttpPost("{id}/address")]
        public async Task<ActionResult> AddAddress(int id, [FromBody] CreateAddressDto addressDto)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != id && userRole != "Admin" && userRole != "Moderator")
                return Forbid();

            int addressId = await _service.AddAddressAsync(addressDto, id);
            return Created($"Address created with id:{addressId}", null);
        }

        // DELETE api/<UsersController>/5/address
        [HttpDelete("{id}/address")]        
        public async Task<ActionResult> DeleteAddress(int id, int addressId)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != id && userRole != "Admin" && userRole != "Moderator")
                return Forbid();

            await _service.DeleteAddressAsync(id, addressId);
            return NoContent();
        }
    }
}
