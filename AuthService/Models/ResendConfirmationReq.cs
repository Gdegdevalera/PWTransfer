using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class ResendConfirmationReq
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
