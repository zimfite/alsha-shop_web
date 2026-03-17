using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Номер телефона обязателен")]
        [Phone(ErrorMessage = "Некорректный номер телефона")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Некорректный email адрес")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имя обязательно")]
        [MaxLength(150, ErrorMessage = "Имя не должно превышать 150 символов")]
        public string FName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Фамилия обязательна")]
        [MaxLength(200, ErrorMessage = "Фамилия не должна превышать 200 символов")]
        public string LName { get; set; } = string.Empty;
        public string? SName { get; set; } // Отчество
        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? PhoneVerifiedAt { get; set; }
        public string? EmailVerificationToken { get; set; }
        public string? PhoneVerificationCode { get; set; }
        [Range(0, 2, ErrorMessage = "Неверное значение пола")]
        public int Gender { get; set; }
    }
}