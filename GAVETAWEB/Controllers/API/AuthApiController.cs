
using GAVETAWEB.DTOs;
using GAVETAWEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace GAVETAWEB.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly Contexto _context;
        private readonly IConfiguration _configuration;

        public AuthApiController(Contexto context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDTO dados)
        {
            var usuario = _context.Usuario.FirstOrDefault(u =>
                u.Email == dados.Email &&
                u.Senha == dados.Senha);

            if (usuario == null)
            {
                return Unauthorized(new
                {
                    mensagem = "E-mail ou senha inválidos."
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.Tipo)
            };

            var chave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credenciais = new SigningCredentials(
                chave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: credenciais
            );

            return Ok(new
            {
                mensagem = "Login realizado com sucesso!",
                token = new JwtSecurityTokenHandler().WriteToken(token),
                usuario = new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email,
                    usuario.Tipo,
                    usuario.Saldo,
                    usuario.FotoPerfil
                }
            });
        }
        [Authorize]
        [HttpGet("perfil")]
        public IActionResult Perfil()
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (idUsuario == null)
            {
                return Unauthorized();
            }

            var usuario = _context.Usuario.FirstOrDefault(u =>
                u.Id == int.Parse(idUsuario));

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Tipo,
                usuario.Saldo,
                usuario.FotoPerfil
            });
        }
    }
}

