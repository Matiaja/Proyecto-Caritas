using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
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
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                CenterId = u.CenterId
            }).ToList();

            return Ok(userDTOs);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<UserDTO>>> GetUsersByFilter(
            [FromQuery] int? centerId = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? order = "asc")

        {
            var users = await _context.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role,
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    CenterId = u.CenterId
                })
                .ToListAsync();

            if (centerId.HasValue)
            {
                users = users.Where(u => u.CenterId == centerId).ToList();
            }

            if (sortBy == null)
            {
                return users;
            }
            if (sortBy == "userName")
            {
                if (order == "asc")
                {
                    return users.OrderBy(u => u.UserName).ToList();
                }
                else if (order == "desc")
                {
                    return users.OrderByDescending(u => u.UserName).ToList();
                }
            }
            return BadRequest(new
            {
                Status = "400",
                Error = "Bad Request",
                Message = "Invalid sortBy parameter."
            });
        }


        //Creo que habria que eliminar este endpoint
        [HttpGet("user-with-center")]
        public async Task<ActionResult<IEnumerable<UserCenterDTO>>> GetAllUserWithCenter()
        {
            var users = await _userManager.Users.ToListAsync();
            if (!users.Any())
            {
                return NotFound(new { message = "No users found." });
            }
            var userDTOs = users
            .Select(u => new UserCenterDTO
            {
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                CenterName = u.Center?.Name ?? string.Empty
            }).ToList();
            return Ok(userDTOs);
        }

        [HttpGet("all-user-no-admin")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUserNoAdmin()
        {
            var users = await _userManager.Users.Where(u => u.Role != "Admin")
                .Include(u => u.Center)
                .ToListAsync();
            if (!users.Any())
            {
                return NotFound(new { message = "No users found." });
            }
            var userDTOs = users.Select(u => new UserDTO
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                CenterId = u.CenterId,
                CenterName = u.Center?.Name ?? string.Empty
            }).ToList();
            return Ok(userDTOs);
        }

        // GET: api/User/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(string id)
        {
            var user = await _userManager.Users
                .Where(u => u.Id == id)
                .Include(u => u.Center)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userDTO = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                CenterId = user.CenterId,
                CenterName = user.Center?.Name ?? string.Empty
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
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
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
            if (string.IsNullOrWhiteSpace(userRegisterDTO.FirstName) || string.IsNullOrWhiteSpace(userRegisterDTO.LastName) || string.IsNullOrWhiteSpace(userRegisterDTO.Role) || string.IsNullOrWhiteSpace(userRegisterDTO.PhoneNumber))
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
            userRegisterDTO.Role = char.ToUpper(userRegisterDTO.Role[0]) + userRegisterDTO.Role.Substring(1).ToLower(); // Normalize role name

            var user = new User
            {
                UserName = userRegisterDTO.UserName,
                Email = userRegisterDTO.Email,
                FirstName = userRegisterDTO.FirstName,
                LastName = userRegisterDTO.LastName,
                Role = userRegisterDTO.Role,
                PhoneNumber = userRegisterDTO.PhoneNumber,
                CenterId = userRegisterDTO.CenterId
            };

            var result = await _userManager.CreateAsync(user, userRegisterDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Normalizar el nombre del rol antes de asignarlo
            var normalizedRole = _roleManager.NormalizeKey(userRegisterDTO.Role);
            var rolassign = await _userManager.AddToRoleAsync(user, normalizedRole);
            if (!rolassign.Succeeded)
            {
                return BadRequest(rolassign.Errors);
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

            var centerId = await GetUserCenterId(user.Id);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("SuperSecureKey1234!·$%&/()=asdfasdf"); // Use a secure key

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString, centerId = centerId, userId = user.Id });
        }



        // Creo que la actualizacion de la contraseña conviene hacerla por otra ruta por el tema del hasheo de la contraseña
        // PUT: api/User/{id}
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, UserDTO userDTO)
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
            if (string.IsNullOrWhiteSpace(userDTO.UserName) || string.IsNullOrWhiteSpace(userDTO.FirstName) || string.IsNullOrWhiteSpace(userDTO.LastName) || string.IsNullOrWhiteSpace(userDTO.Role) || string.IsNullOrWhiteSpace(userDTO.PhoneNumber))
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
            userDTO.Role = char.ToUpper(userDTO.Role[0]) + userDTO.Role.Substring(1).ToLower(); // Normalize role name

            // Update user properties
            user.Id = userDTO.Id;
            user.UserName = userDTO.UserName;
            user.Email = userDTO.Email;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.Role = userDTO.Role;
            user.PhoneNumber = userDTO.PhoneNumber;
            user.CenterId = userDTO.CenterId;

            // Actualizar la contraseña si se proporciona
            //if (!string.IsNullOrWhiteSpace(userDTO.Password))
            //{
            //    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            //    if (!removePasswordResult.Succeeded)
            //    {
            //        return BadRequest(removePasswordResult.Errors);
            //    }

            //    var addPasswordResult = await _userManager.AddPasswordAsync(user, userDTO.Password);
            //    if (!addPasswordResult.Succeeded)
            //    {
            //        return BadRequest(addPasswordResult.Errors);
            //    }
            //}

            // Update user in the database
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Remove all roles and add the new role
            var currentRoles = await _userManager.GetRolesAsync(user);
            var normalizedRole = _roleManager.NormalizeKey(userDTO.Role);
            if (!currentRoles.Any(role => _roleManager.NormalizeKey(role) == normalizedRole))
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

        private async Task<int?> GetUserCenterId(string id)
        {
            return await _context.Users
                .Where(u => u.Id == id)
                .Select(u => u.CenterId)
                .FirstOrDefaultAsync();
        }
    }
}
