using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace VerificacaoDeSenha.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        public UserController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("registrar")]

        public async Task<IActionResult> Registrar(UserRegister request)
        {
            if (_context.Users.Any(u => u.email == request.Email))
            {
                return BadRequest("Usuario já existe");
            }

            CriarSenhaHash(request.SenhaForte,
            out byte[] senhaHash,
            out byte[] senha);

            var user = new User
            {
                email = request.Email,
                senhaHash = senhaHash,
                senha = senha,
                verificaToken = CriarTokenAleatorio()

            };

            EnviarEmail(request.Email, request.SenhaForte, user.verificaToken);
                
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Usuario criado com sucesso");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == request.Email);
            if (user == null)
            {
                return BadRequest("Email ou senha incorreto.");
            }

            if (!VerificarHashSenha(request.SenhaForte, user.senhaHash, user.senha))
            {
                return BadRequest("Email ou Senha incorreto.");
            }

            if (user.dataVerificado == null)
            {
                return BadRequest("Usuario não verificado.");
            }

            return Ok($"Bem vindo, {user.email}. :)");
        }

        [HttpPost("verificar")]
        public async Task<IActionResult> Verificar(string token)
        {
           
            var user = await _context.Users.FirstOrDefaultAsync(u => u.verificaToken == token);
            if (user == null)
            {
                return BadRequest("Token invalido.");
            }

            user.dataVerificado = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok("Usuario verificado.");
        }

        [HttpPost("Alterar-Senha")]
        public async Task<IActionResult> EsquecerSenha(UserLogin request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == request.Email);
            if (user == null)
            {
                return BadRequest("Usuario não encontrado.");
            }

            user.reseteSenha = CriarTokenAleatorio();
            user.expiraToken = DateTime.Now.AddDays(1);
            EnviarEmail(request.Email, request.SenhaForte, user.reseteSenha);
            await _context.SaveChangesAsync();

            return Ok("Email enviado.");
        }


        [HttpPost("Resetar-Senha-com-Token")]
        public async Task<IActionResult> ResetarSenha(ResettPassword request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.reseteSenha == request.Token);
            if (user == null || user.expiraToken < DateTime.Now)
            {
                return BadRequest("Token invalido");
            }

            CriarSenhaHash(request.SenhaForte, out byte[] senhaHash, out byte[] senha);

            user.senhaHash = senhaHash;
            user.senha = senha;
            user.reseteSenha = null;
            user.expiraToken = null;

            await _context.SaveChangesAsync();

            return Ok("Senha alterada com sucesso.");
        }


        private static bool VerificarHashSenha(string senhaForte, byte[] senhaHash, byte[] senha)
        {
            using (HMACSHA512? hmac = new HMACSHA512(senha))
            {
                var computedHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(senhaForte));
                return computedHash.SequenceEqual(senhaHash);
            }
        }
     
        private static void CriarSenhaHash(string SenhaForte, out byte[] senhaHash, out byte[] senha)
        {
            using var hmac = new HMACSHA512();
            senha = hmac.Key;
            senhaHash = hmac
                .ComputeHash(System.Text.Encoding.UTF8.GetBytes(SenhaForte));
        }

        private static string CriarTokenAleatorio()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        }

        private static void EnviarEmail(string emaill, string senha, string token)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(emaill));
            email.To.Add(MailboxAddress.Parse(emaill));
            email.Subject = ("Token de acesso.");
            email.Body = new TextPart(TextFormat.Html) { Text = token };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.ethereal.email", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(emaill, senha);
            smtp.Send(email);
            smtp.Disconnect(true);

        }

    }
}
