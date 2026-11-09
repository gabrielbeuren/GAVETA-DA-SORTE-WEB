
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

        [Authorize]
        [HttpGet("grupos")]
        public IActionResult MeusGrupos()
        {
            var idUsuario = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var grupos = _context.ParticipanteGrupo
                .Where(p => p.UsuarioId == idUsuario)
                .Select(p => new
                {
                    p.Grupo.Id,
                    p.Grupo.Nome,
                    p.Grupo.Descricao,
                    p.Grupo.DataCriacao,
                    p.Grupo.CriadorId,
                    p.Grupo.AdministradorId
                })
                .ToList();

            return Ok(grupos);
        }

        [Authorize]
        [HttpPost("criar-grupo")]
        public IActionResult CriarGrupo(CriarGrupoDTO dados)
        {
            var idUsuario = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var grupo = new Grupo
            {
                Nome = dados.Nome,
                Descricao = dados.Descricao,
                DataCriacao = DateTime.Now,
                CriadorId = idUsuario,
                AdministradorId = idUsuario
            };

            _context.Grupo.Add(grupo);
            _context.SaveChanges();

            var participante = new ParticipanteGrupo
            {
                UsuarioId = idUsuario,
                GrupoId = grupo.Id
            };

            _context.ParticipanteGrupo.Add(participante);
            _context.SaveChanges();

            return Ok(new
            {
                mensagem = "Grupo criado com sucesso!",
                grupo
            });
        }

        [Authorize]
        [HttpGet("detalhes-grupo/{id}")]
        public IActionResult DetalhesGrupo(int id)
        {
            var idUsuario = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var grupo = _context.Grupo
                .FirstOrDefault(g => g.Id == id);

            if (grupo == null)
            {
                return NotFound(new
                {
                    mensagem = "Grupo não encontrado."
                });
            }

            bool participa = _context.ParticipanteGrupo.Any(p =>
                p.GrupoId == id && p.UsuarioId == idUsuario);

            if (!participa)
            {
                return Forbid();
            }

            var administrador = _context.Usuario
                .Where(u => u.Id == grupo.AdministradorId)
                .Select(u => new
                {
                    u.Id,
                    u.Nome
                })
                .FirstOrDefault();

            var participantes = _context.ParticipanteGrupo
                .Where(p => p.GrupoId == id)
                .Select(p => new
                {
                    p.Usuario.Id,
                    p.Usuario.Nome
                })
                .ToList();

            var totalDepositos = _context.Deposito
                .Where(d => d.GrupoId == id)
                .Sum(d => (decimal?)d.Valor) ?? 0;

            return Ok(new
            {
                grupo.Id,
                grupo.Nome,
                grupo.Descricao,
                grupo.DataCriacao,
                administrador,
                participantes,
                totalDepositos
            });
        }
    }
}

