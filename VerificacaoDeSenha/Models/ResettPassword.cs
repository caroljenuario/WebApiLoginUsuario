using System.ComponentModel.DataAnnotations;
namespace VerificacaoDeSenha.Models
{
    public class ResettPassword
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required, MinLength(8, ErrorMessage = "Por favor, inserir ao menos 8 caracteres.")]
        public string SenhaForte { get; set; } = string.Empty;
        [Required, Compare("SenhaForte")]
        public string ConfirmaSenha { get; set; } = string.Empty;
    }
}
