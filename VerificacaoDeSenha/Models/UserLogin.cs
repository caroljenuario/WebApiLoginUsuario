using System.ComponentModel.DataAnnotations;
namespace VerificacaoDeSenha.Models
{
    public class UserLogin
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string SenhaForte { get; set; } = string.Empty;
        
    }
}
