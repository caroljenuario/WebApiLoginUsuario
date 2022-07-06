using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

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

        public async Task <IActionResult> Registrar(UserRegister request)
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

        [HttpPost("Esquecer-Senha")]
        public async Task<IActionResult> EsquecerSenha(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
            if (user == null)
            {
                return BadRequest("Usuario nao encontrado.");
            }

            user.reseteSenha = CriarTokenAleatorio();
            user.expiraToken = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            return Ok("Resete sua senha.");
        }


        [HttpPost("Resetar-Senha")]
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


        private bool VerificarHashSenha(string senhaForte, byte[] senhaHash, byte[] senha)
        {
            using (var hmac = new HMACSHA512(senha))
            {
                var computedHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(senhaForte));
                return computedHash.SequenceEqual(senhaHash);
            }
        }


        private void CriarSenhaHash(string SenhaForte, out byte[] senhaHash, out byte[] senha)
        {
            using (var hmac = new HMACSHA512())
            {
                senha = hmac.Key;
                senhaHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(SenhaForte));
            }
        }

        private string CriarTokenAleatorio()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        }

    }
}
