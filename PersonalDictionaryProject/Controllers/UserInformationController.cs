using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalDictionaryProject.Models;
using PersonalDictionaryProject.Dtos;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace PersonalDictionaryProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserInformationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserInformationController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserByToken()
        {
            var token = Request.Headers["Authorization"].ToString();
            Console.WriteLine($"Received Token: {token}");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"Extracted userId: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.UserName,
                user.Email,
                user.PhoneNumber,
            });
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.UserName,
                    u.Email,
                    u.PhoneNumber
                })
                .ToListAsync();

            return Ok(users);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserbyId(string id)
        {
            var users = await _userManager.Users.Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.UserName,
                    u.Email,
                    u.PhoneNumber
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound("User not found");

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("User information updated successfully");
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound("User not found");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("Password changed successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("User deleted successfully");
        }


        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok(new { message = "User registered successfully!" });
        }
    }
}
