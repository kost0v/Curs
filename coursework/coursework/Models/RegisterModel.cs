using System.ComponentModel.DataAnnotations;

namespace coursework.Models
{
    public class RegisterModel
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
