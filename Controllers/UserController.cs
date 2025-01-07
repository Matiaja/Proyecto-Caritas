using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            if (!users.Any())
            {
                return NotFound(new { message = "No users found." });
            }
            var userDTOs = users.Select(u => new UserDTO
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                Phone = u.Phone,
                CenterId = u.CenterId
            }).ToList();

            return Ok(userDTOs);
        }

        // GET: api/User/{id}
        [HttpGet("id/{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userDTO = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Phone = user.Phone,
                CenterId = user.CenterId
            };

            return Ok(userDTO);
        }

        // GET: api/User/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserDTO>> GetUserByUsername(string username)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userDTO = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Phone = user.Phone,
                CenterId = user.CenterId
            };

            return Ok(userDTO);
        }

        // POST: api/User/register
        [HttpPost("register")]
        public async Task<IActionResult> AddUser(UserRegisterDTO userRegisterDTO)
        {
            if (string.IsNullOrWhiteSpace(userRegisterDTO.Email))
            {
                return BadRequest(new { message = "Email is required." });
            }
            if (string.IsNullOrWhiteSpace(userRegisterDTO.FirstName) || string.IsNullOrWhiteSpace(userRegisterDTO.LastName) || string.IsNullOrWhiteSpace(userRegisterDTO.Role) || string.IsNullOrWhiteSpace(userRegisterDTO.Phone))
            {
                return BadRequest(new { message = "FirstName, LastName, Role, and Phone are required fields." });
            }
            if (!await _roleManager.RoleExistsAsync(userRegisterDTO.Role))
            {
                return BadRequest(new { message = "Specified role does not exist." });
            }
            if (userRegisterDTO.CenterId.HasValue && !await _context.Centers.AnyAsync(c => c.Id == userRegisterDTO.CenterId.Value))
            {
                return BadRequest(new { message = "Specified Center does not exist." });
            }
            var user = new User
            {
                UserName = userRegisterDTO.UserName,
                Email = userRegisterDTO.Email,
                FirstName = userRegisterDTO.FirstName,
                LastName = userRegisterDTO.LastName,
                Role = userRegisterDTO.Role,
                Phone = userRegisterDTO.Phone,
                CenterId = userRegisterDTO.CenterId
            };

            var result = await _userManager.CreateAsync(user, userRegisterDTO.Password);
            var rolassign = await _userManager.AddToRoleAsync(user, userRegisterDTO.Role);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "User registered successfully" });
        }

        // POST: api/User/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDTO userLoginDTO)
        {
            var user = await _userManager.FindByNameAsync(userLoginDTO.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, userLoginDTO.Password))
            {
                return Unauthorized(new { message = "Invalid login attempt" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("SuperSecureKey1234!·$%&/()=asdfasdf"); // Use a secure key

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role)
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString });
        }

        // PUT: api/User/{id}
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, UserRegisterDTO userDTO)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            if (string.IsNullOrWhiteSpace(userDTO.Email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            if (string.IsNullOrWhiteSpace(userDTO.UserName) || string.IsNullOrWhiteSpace(userDTO.FirstName) || string.IsNullOrWhiteSpace(userDTO.LastName) || string.IsNullOrWhiteSpace(userDTO.Role) || string.IsNullOrWhiteSpace(userDTO.Phone))
            {
                return BadRequest(new { message = "UserName, FirstName, LastName, Role, and Phone are required fields." });
            }
            if (!await _roleManager.RoleExistsAsync(userDTO.Role))
            {
                return BadRequest(new { message = "Specified role does not exist." });
            }
            if (userDTO.CenterId.HasValue && !await _context.Centers.AnyAsync(c => c.Id == userDTO.CenterId.Value))
            {
                return BadRequest(new { message = "Specified Center does not exist." });
            }

            // Update user properties
            user.UserName = userDTO.UserName;
            user.Email = userDTO.Email;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.Role = userDTO.Role;
            user.Phone = userDTO.Phone;
            user.CenterId = userDTO.CenterId;

            // Actualizar la contraseña si se proporciona
            if (!string.IsNullOrWhiteSpace(userDTO.Password))
            {
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    return BadRequest(removePasswordResult.Errors);
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, userDTO.Password);
                if (!addPasswordResult.Succeeded)
                {
                    return BadRequest(addPasswordResult.Errors);
                }
            }

            // Update user in the database
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Remove all roles and add the new role
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(userDTO.Role))
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRolesResult.Succeeded)
                {
                    return BadRequest(removeRolesResult.Errors);
                }

                var roleResult = await _userManager.AddToRoleAsync(user, userDTO.Role);
                if (!roleResult.Succeeded)
                {
                    return BadRequest(roleResult.Errors);
                }
            }

            return Ok(new { message = "User updated successfully" });
        }

        // DELETE: api/User/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "User deleted successfully" });
        }
    }
}
