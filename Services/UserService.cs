using System.Data;
using BCrypt.Net;
using Dapper;
using Microsoft.CodeAnalysis.Scripting;
using Npgsql;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class UserService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<UserService> _logger;

        public UserService(DatabaseService databaseService, ILogger<UserService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<User> RegisterUser(User user)
        {
            try
            {
                _logger.LogInformation($"Начало регистрации пользователя с телефоном: {user.Phone}");

                var existingUser = await GetUserByPhone(user.Phone);
                if (existingUser != null)
                {
                    throw new Exception("Пользователь с таким номером телефона уже существует");
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                _logger.LogDebug($"Пароль хеширован для пользователя: {user.Phone}");

                var sql = @"
                    INSERT INTO users (
                        phone, email, fname, lname, sname, 
                        password, gender, created_at, updated_at
                    ) VALUES (
                        @Phone, @Email, @Fname, @Lname, @Sname, 
                        @Password, @Gender, @CreatedAt, @UpdatedAt
                    ) RETURNING id";

                user.Password = hashedPassword;
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    user.Email = null;
                }

                var userId = await _databaseService.QuerySingleAsync<int>(sql, user);
                user.Id = userId;

                _logger.LogInformation($"Пользователь успешно зарегистрирован. ID: {userId}");
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при регистрации пользователя {user.Phone}");
                throw;
            }
        }

        public async Task<User?> GetUserByPhone(string phone)
        {
            try
            {
                var formattedPhone = FormatPhoneNumber(phone);
                var sql = "SELECT * FROM users WHERE phone = @Phone";
                var user = await _databaseService.QuerySingleAsync<User>(sql, new { Phone = formattedPhone });

                if (user != null)
                {
                    _logger.LogDebug($"Найден пользователь по телефону: {formattedPhone}, ID: {user.Id}");
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при поиске пользователя по телефону: {phone}");
                return null;
            }
        }

        public async Task<User?> GetUserById(int id)
        {
            try
            {
                var sql = "SELECT * FROM users WHERE id = @Id";
                return await _databaseService.QuerySingleAsync<User>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при поиске пользователя по ID: {id}");
                return null;
            }
        }

        public async Task<User?> Authenticate(string phone, string password)
        {
            try
            {
                var formattedPhone = FormatPhoneNumber(phone);
                _logger.LogInformation($"Попытка аутентификации пользователя: {formattedPhone}");

                var user = await GetUserByPhone(formattedPhone);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь не найден: {formattedPhone}");
                    return null;
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

                if (isPasswordValid)
                {
                    _logger.LogInformation($"Аутентификация успешна для пользователя: {formattedPhone}");
                    return user;
                }
                else
                {
                    _logger.LogWarning($"Неверный пароль для пользователя: {formattedPhone}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при аутентификации пользователя: {phone}");
                return null;
            }
        }

        public string FormatPhoneNumber(string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone))
                    return phone;

                var digits = new string(phone.Where(char.IsDigit).ToArray());

                if (digits.Length == 11)
                {
                    if (digits.StartsWith("8"))
                    {
                        return "+7" + digits.Substring(1);
                    }
                    else if (digits.StartsWith("7"))
                    {
                        return "+" + digits;
                    }
                }
                else if (digits.Length == 10)
                {
                    return "+7" + digits;
                }

                return phone;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при форматировании номера телефона: {phone}");
                return phone;
            }
        }

        public async Task<bool> UserExists(string phone)
        {
            try
            {
                var formattedPhone = FormatPhoneNumber(phone);
                var sql = "SELECT COUNT(1) FROM users WHERE phone = @Phone";
                var count = await _databaseService.QuerySingleAsync<int>(sql, new { Phone = formattedPhone });
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при проверке существования пользователя: {phone}");
                return false;
            }
        }

        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.UtcNow;

                var sql = @"
                    UPDATE users 
                    SET email = @Email, 
                        fname = @Fname, 
                        lname = @Lname, 
                        sname = @Sname,
                        gender = @Gender,
                        updated_at = @UpdatedAt
                    WHERE id = @Id";

                var affectedRows = await _databaseService.ExecuteAsync(sql, user);
                _logger.LogInformation($"Обновлена информация о пользователе ID: {user.Id}, затронуто строк: {affectedRows}");

                return affectedRows > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}