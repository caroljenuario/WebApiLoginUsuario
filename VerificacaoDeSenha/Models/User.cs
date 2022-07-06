namespace VerificacaoDeSenha.Models
{
    public class User
    {
        public int id { get; set; }
        public string email { get; set; } = string.Empty;
        public byte[] senhaHash { get; set; }
        public byte[] senha { get; set; }
        public string? verificaToken { get; set; }
        public DateTime? dataVerificado { get; set; }
        public string? reseteSenha { get; set; }   
        public DateTime? expiraToken { get; set; }   

    }
}
