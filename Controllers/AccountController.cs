using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
        {
            try
            {
                _logger.LogInformation($"Начало регистрации пользователя с телефоном: {registrationDto.Phone}");

                if (string.IsNullOrWhiteSpace(registrationDto.Phone))
                {
                    return BadRequest(new { success = false, message = "Номер телефона обязателен" });
                }

                if (string.IsNullOrWhiteSpace(registrationDto.FName))
                {
                    return BadRequest(new { success = false, message = "Имя обязательно" });
                }

                if (string.IsNullOrWhiteSpace(registrationDto.LName))
                {
                    return BadRequest(new { success = false, message = "Фамилия обязательна" });
                }

                if (string.IsNullOrWhiteSpace(registrationDto.Password))
                {
                    return BadRequest(new { success = false, message = "Пароль обязателен" });
                }

                if (registrationDto.Password.Length < 8)
                {
                    return BadRequest(new { success = false, message = "Пароль должен содержать минимум 8 символов" });
                }

                if (registrationDto.Password != registrationDto.ConfirmPassword)
                {
                    return BadRequest(new { success = false, message = "Пароли не совпадают" });
                }

                if (!registrationDto.AcceptPrivacyPolicy)
                {
                    return BadRequest(new { success = false, message = "Необходимо принять политику конфиденциальности" });
                }

                var formattedPhone = _userService.FormatPhoneNumber(registrationDto.Phone);

                var userExists = await _userService.UserExists(formattedPhone);
                if (userExists)
                {
                    return BadRequest(new { success = false, message = "Пользователь с таким номером телефона уже существует" });
                }
                var user = new User
                {
                    Phone = formattedPhone,
                    Email = string.IsNullOrWhiteSpace(registrationDto.Email) ? null : registrationDto.Email.Trim(),
                    FName = registrationDto.FName.Trim(),
                    LName = registrationDto.LName.Trim(),
                    SName = string.IsNullOrWhiteSpace(registrationDto.SName) ? null : registrationDto.SName.Trim(),
                    Password = registrationDto.Password,
                    Gender = registrationDto.Gender
                };

                var registeredUser = await _userService.RegisterUser(user);

                _logger.LogInformation($"Пользователь {formattedPhone} успешно зарегистрирован с ID: {registeredUser.Id}");

                return Ok(new
                {
                    success = true,
                    message = "Регистрация успешна!",
                    data = new
                    {
                        id = registeredUser.Id,
                        phone = registeredUser.Phone,
                        email = registeredUser.Email,
                        fname = registeredUser.FName,
                        lname = registeredUser.LName,
                        sname = registeredUser.SName
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginDto.Phone) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return BadRequest(new { success = false, message = "Необходимо указать телефон и пароль" });
                }

                var formattedPhone = _userService.FormatPhoneNumber(loginDto.Phone);

                var user = await _userService.Authenticate(formattedPhone, loginDto.Password);

                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Неверный номер телефона или пароль" });
                }

                _logger.LogInformation($"Успешный вход пользователя: {formattedPhone}, ID: {user.Id}");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, $"{user.FName} {user.LName}"),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Phone", user.Phone),
                    new Claim("Email", user.Email ?? ""),
                    new Claim("FirstName", user.FName),
                    new Claim("LastName", user.LName)
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", $"{user.FName} {user.LName}");

                _logger.LogInformation($"Пользователь {user.Id} аутентифицирован. Claims установлены.");

                return Ok(new
                {
                    success = true,
                    message = "Вход выполнен успешно",
                    data = new
                    {
                        id = user.Id,
                        phone = user.Phone,
                        email = user.Email,
                        fname = user.FName,
                        lname = user.LName,
                        sname = user.SName,
                        gender = user.Gender
                    }
                });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Ошибка при входе" });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                HttpContext.Session.Clear();

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                _logger.LogInformation("Пользователь вышел из системы");

                return Ok(new
                {
                    success = true,
                    message = "Выход выполнен успешно"
                });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Ошибка при выходе" });
            }
        }

        [HttpGet("check-auth")]
        public IActionResult CheckAuth()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity?.Name;

            return Ok(new
            {
                isAuthenticated,
                userId,
                userName,
                claims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
            });
        }

        [HttpGet("check-phone/{phone}")]
        public async Task<IActionResult> CheckPhoneExists(string phone)
        {
            try
            {
                var exists = await _userService.UserExists(phone);
                return Ok(new { exists = exists });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Ошибка при проверке телефона" });
            }
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "Пользователь не найден" });
                }

                user.Password = string.Empty;

                return Ok(new
                {
                    success = true,
                    data = user
                });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Ошибка при получении пользователя" });
            }
        }
    }

    public class UserRegistrationDto
    {
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FName { get; set; } = string.Empty;
        public string LName { get; set; } = string.Empty;
        public string? SName { get; set; }
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public int Gender { get; set; }
        public bool AcceptPrivacyPolicy { get; set; }
    }

    public class LoginDto
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}