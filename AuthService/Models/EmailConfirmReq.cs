using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class EmailConfirmReq
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
