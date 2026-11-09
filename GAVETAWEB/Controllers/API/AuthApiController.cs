
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

        [Authorize]
        [HttpPost("adicionar-participante")]
        public IActionResult AdicionarParticipante(AdicionarParticipanteDTO dados)
        {
            var idUsuario = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var grupo = _context.Grupo
                .FirstOrDefault(g => g.Id == dados.GrupoId);

            if (grupo == null)
            {
                return NotFound(new
                {
                    mensagem = "Grupo não encontrado."
                });
            }

            if (grupo.AdministradorId != idUsuario)
            {
                return Forbid();
            }

            var usuario = _context.Usuario
                .FirstOrDefault(u => u.Id == dados.UsuarioId);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem = "Usuário não encontrado."
                });
            }

            bool jaExiste = _context.ParticipanteGrupo.Any(p =>
                p.UsuarioId == dados.UsuarioId &&
                p.GrupoId == dados.GrupoId);

            if (jaExiste)
            {
                return BadRequest(new
                {
                    mensagem = "Este usuário já faz parte do grupo."
                });
            }

            var participante = new ParticipanteGrupo
            {
                UsuarioId = dados.UsuarioId,
                GrupoId = dados.GrupoId
            };

            _context.ParticipanteGrupo.Add(participante);
            _context.SaveChanges();

            return Ok(new
            {
                mensagem = "Participante adicionado com sucesso!"
            });
        }

        [Authorize]
        [HttpPost("depositar")]
        public IActionResult Depositar(DepositoDTO dados)
        {
            var idUsuario = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var usuario = _context.Usuario
                .FirstOrDefault(u => u.Id == idUsuario);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem = "Usuário não encontrado."
                });
            }

            var grupo = _context.Grupo
                .FirstOrDefault(g => g.Id == dados.GrupoId);

            if (grupo == null)
            {
                return NotFound(new
                {
                    mensagem = "Grupo não encontrado."
                });
            }

            bool participa = _context.ParticipanteGrupo.Any(p =>
                p.GrupoId == dados.GrupoId &&
                p.UsuarioId == idUsuario);

            if (!participa)
            {
                return BadRequest(new
                {
                    mensagem = "Você não participa deste grupo."
                });
            }

            if (dados.Valor <= 0)
            {
                return BadRequest(new
                {
                    mensagem = "Informe um valor válido."
                });
            }

            if (usuario.Saldo < dados.Valor)
            {
                return BadRequest(new
                {
                    mensagem = "Saldo insuficiente."
                });
            }

            var deposito = new Deposito
            {
                UsuarioId = idUsuario,
                GrupoId = dados.GrupoId,
                Valor = dados.Valor,
                DataDeposito = DateTime.Now
            };

            usuario.Saldo -= dados.Valor;

            _context.Deposito.Add(deposito);
            _context.Usuario.Update(usuario);
            _context.SaveChanges();

            return Ok(new
            {
                mensagem = "Depósito realizado com sucesso!",
                valorDepositado = dados.Valor,
                saldoRestante = usuario.Saldo
            });
        }

        [Authorize]
        [HttpPost("realizar-sorteio/{id}")]
        public IActionResult RealizarSorteio(int id)
        {
            var idUsuario = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var grupo = _context.Grupo.FirstOrDefault(g => g.Id == id);

            if (grupo == null)
                return NotFound(new { mensagem = "Grupo não encontrado." });

            if (grupo.AdministradorId != idUsuario)
                return Forbid();

            var participantes = _context.ParticipanteGrupo
                .Where(p => p.GrupoId == id)
                .ToList();

            if (participantes.Count == 0)
                return BadRequest(new { mensagem = "Não existem participantes no grupo." });

            var depositos = _context.Deposito
                .Where(d => d.GrupoId == id)
                .ToList();

            decimal premio = depositos.Sum(d => d.Valor);

            if (premio <= 0)
                return BadRequest(new { mensagem = "Não existem depósitos para realizar o sorteio." });

            var sorteado = participantes[
                new Random().Next(participantes.Count)];

            var vencedor = _context.Usuario
                .FirstOrDefault(u => u.Id == sorteado.UsuarioId);

            if (vencedor == null)
                return NotFound(new { mensagem = "Vencedor não encontrado." });

            vencedor.Saldo += premio;

            var sorteio = new Sorteio
            {
                GrupoId = id,
                UsuarioId = vencedor.Id,
                ValorPremio = premio,
                DataSorteio = DateTime.Now
            };

            _context.Sorteio.Add(sorteio);
            _context.Deposito.RemoveRange(depositos);

            _context.SaveChanges();

            return Ok(new
            {
                mensagem = "Sorteio realizado com sucesso!",
                vencedor = vencedor.Nome,
                valorPremio = premio
            });
        }

        [Authorize]
        [HttpDelete("remover-participante/{id}")]
        public IActionResult RemoverParticipante(int id)
        {
            var idUsuario = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var participante = _context.ParticipanteGrupo
                .FirstOrDefault(p => p.Id == id);

            if (participante == null)
                return NotFound(new { mensagem = "Participante não encontrado." });

            var grupo = _context.Grupo
                .FirstOrDefault(g => g.Id == participante.GrupoId);

            if (grupo == null)
                return NotFound(new { mensagem = "Grupo não encontrado." });

            if (grupo.AdministradorId != idUsuario)
                return Forbid();

            if (participante.UsuarioId == grupo.AdministradorId)
                return BadRequest(new { mensagem = "O administrador não pode remover a si mesmo." });

            var depositos = _context.Deposito
                .Where(d => d.GrupoId == grupo.Id &&
                            d.UsuarioId == participante.UsuarioId)
                .ToList();

            decimal valorDevolvido = depositos.Sum(d => d.Valor);

            var usuarioRemovido = _context.Usuario
                .FirstOrDefault(u => u.Id == participante.UsuarioId);

            if (usuarioRemovido != null)
                usuarioRemovido.Saldo += valorDevolvido;

            _context.Deposito.RemoveRange(depositos);
            _context.ParticipanteGrupo.Remove(participante);

            _context.SaveChanges();

            return Ok(new
            {
                mensagem = "Participante removido com sucesso.",
                valorDevolvido = valorDevolvido
            });
        }

        [Authorize]
        [HttpPut("editar-grupo/{id}")]
        public IActionResult EditarGrupo(int id, EditarGrupoDTO dados)
        {
            var idUsuario = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var grupo = _context.Grupo
                .FirstOrDefault(g => g.Id == id);

            if (grupo == null)
                return NotFound(new { mensagem = "Grupo não encontrado." });

            if (grupo.AdministradorId != idUsuario)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dados.Nome))
                return BadRequest(new { mensagem = "Informe o nome do grupo." });

            grupo.Nome = dados.Nome;
            grupo.Descricao = dados.Descricao;

            _context.SaveChanges();

            return Ok(new
            {
                mensagem = "Grupo atualizado com sucesso!",
                grupo.Id,
                grupo.Nome,
                grupo.Descricao
            });
        }
    }
}

