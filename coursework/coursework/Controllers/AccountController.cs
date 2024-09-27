using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using coursework.Models;
using Microsoft.Extensions.Logging;

namespace coursework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ShopContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ShopContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Account/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Login == model.Login))
                return Conflict("User already exists");

            var user = new User
            {
                Login = model.Login,
                Password = model.Password, 
                Role = "user"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully", userId = user.Id });
        }

        // POST: api/Account/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);

            if (user == null)
            {
                _logger.LogWarning("Invalid login attempt for user: {Login}", model.Login);
                return Unauthorized("Invalid login or password");
            }

            _logger.LogInformation("User {Login} logged in successfully with userId {UserId}", user.Login, user.Id);
            return Ok(new { role = user.Role, userId = user.Id });
        }

        // POST: api/Account/Logout
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            // успешный ответ
            _logger.LogInformation("User logged out successfully");
            return Ok("Logged out successfully");
        }

        // PUT: api/Account/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, UpdateProfileModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            user.Login = model.Login;
            user.Password = model.Password; // мб надо хешировать

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }
    }
}
