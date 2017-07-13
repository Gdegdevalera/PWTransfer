using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class ForgotPasswordReq
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
