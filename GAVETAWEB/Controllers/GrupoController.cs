using GAVETAWEB.Migrations;
using GAVETAWEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAVETAWEB.Controllers
{
    public class GrupoController : Controller
    {

        private readonly Contexto _context;

        public GrupoController(Contexto context)
        {
            _context = context;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GrupoTelaInicial()
        {
            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario
                .FirstOrDefault(u => u.Email == email);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Usuario");
            }


            var grupos = _context.ParticipanteGrupo
                .Where(p => p.UsuarioId == usuario.Id)
                .Include(p => p.Grupo)
                .Select(p => p.Grupo)
                .ToList();


            return View(grupos);
        }

        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastro(Grupo grupo)
        {
            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            grupo.DataCriacao = DateTime.Now;
            grupo.AdministradorId = usuario.Id;
            _context.Grupo.Add(grupo);
            _context.SaveChanges();

            ParticipanteGrupo participante = new ParticipanteGrupo();

            participante.UsuarioId = usuario.Id;
            participante.GrupoId = grupo.Id;

            _context.ParticipanteGrupo.Add(participante);
            _context.SaveChanges();
            return RedirectToAction("GrupoTelaInicial");
       
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Detalhes(int id)
        {
            var grupo = _context.Grupo.FirstOrDefault(g => g.Id == id);

            if (grupo == null)
            {
                TempData["Mensagem"] = "Este grupo não existe mais.";
                return RedirectToAction("GrupoTelaInicial");
            }

            ViewBag.Participantes = _context.ParticipanteGrupo // mostra a lista de participantes
           .Where(p => p.GrupoId == id)
           .Include(p => p.Usuario)
           .ToList();

            ViewBag.Depositos = _context.Deposito //Mostra a lista de deposito
            .Where(d => d.GrupoId == id)
            .Include(d => d.Usuario)
            .ToList();

            ViewBag.TotalDepositos = _context.Deposito
           .Where(d => d.GrupoId == id)
           .Sum(d => d.Valor);

            var administrador = _context.Usuario
           .FirstOrDefault(u => u.Id == grupo.AdministradorId);

            ViewBag.Administrador = administrador;

            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            ViewBag.EhAdministrador = usuario != null &&
                                       usuario.Id == grupo.AdministradorId;

            return View(grupo);  
        }

        public IActionResult AdicionarParticipante(int id)
        {
            ViewBag.GrupoId = id;

            ViewBag.Usuarios = _context.Usuario.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult AdicionarParticipante(int UsuarioId, int GrupoId)
        {
            bool jaExiste = _context.ParticipanteGrupo.Any(p =>
            p.UsuarioId == UsuarioId &&
            p.GrupoId == GrupoId);

            if (jaExiste)
            {
                ViewBag.GrupoId = GrupoId;
                ViewBag.Usuarios = _context.Usuario.ToList();
                ViewBag.Mensagem = "Este usuário já faz parte do grupo.";

                return View();
            }
            ParticipanteGrupo participante = new ParticipanteGrupo();

            participante.UsuarioId = UsuarioId;
            participante.GrupoId = GrupoId;

            _context.ParticipanteGrupo.Add(participante);
            _context.SaveChanges();

            return RedirectToAction("Detalhes", new { id = GrupoId });
        }

        public IActionResult Deposito(int id)
        {
            ViewBag.GrupoId = id;

            return View();
        }


        [HttpPost]
        public IActionResult Deposito(Deposito deposito)
        {
            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario
                .FirstOrDefault(u => u.Email == email);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Usuario");
            }


            var grupo = _context.Grupo
                .FirstOrDefault(g => g.Id == deposito.GrupoId);

            if (grupo == null)
            {
                TempData["Erro"] = "Grupo não encontrado.";
                return RedirectToAction("GrupoTelaInicial");
            }


            bool participante = _context.ParticipanteGrupo
                .Any(p => p.GrupoId == deposito.GrupoId &&
                          p.UsuarioId == usuario.Id);


            if (!participante)
            {
                TempData["Erro"] = "Você não participa deste grupo.";
                return RedirectToAction("Detalhes", new { id = deposito.GrupoId });
            }


            if (deposito.Valor <= 0)
            {
                TempData["Erro"] = "Informe um valor válido.";
                return RedirectToAction("Deposito", new { id = deposito.GrupoId });
            }


            if (usuario.Saldo < deposito.Valor)
            {
                TempData["Erro"] = "Saldo insuficiente.";
                return RedirectToAction("Deposito", new { id = deposito.GrupoId });
            }


            deposito.UsuarioId = usuario.Id;
            deposito.DataDeposito = DateTime.Now;


            usuario.Saldo -= deposito.Valor;

            deposito.Id = 0;

            _context.Deposito.Add(deposito);

            _context.Usuario.Update(usuario);

            _context.SaveChanges();


            return RedirectToAction("Detalhes", new { id = deposito.GrupoId });
        }


        public IActionResult RealizarSorteio(int id)
        {
            var grupo = _context.Grupo.FirstOrDefault(g => g.Id == id);

            if (grupo == null)
            {
                return RedirectToAction("GrupoTelaInicial");
            }

            var participantes = _context.ParticipanteGrupo
                .Where(p => p.GrupoId == id)
                .ToList();

            if (participantes.Count == 0)
            {
                return RedirectToAction("Detalhes", new { id });
            }

            Random random = new Random();

            var vencedor = participantes[random.Next(participantes.Count)];

            decimal valorPremio = _context.Deposito
                .Where(d => d.GrupoId == id)
                .Sum(d => d.Valor);

            // Credita o prêmio no saldo do vencedor
            var usuarioVencedor = _context.Usuario
                .FirstOrDefault(u => u.Id == vencedor.UsuarioId);

            if (usuarioVencedor != null)
            {
                usuarioVencedor.Saldo += valorPremio;
            }

            // Salva o histórico do sorteio
            Sorteio sorteio = new Sorteio
            {
                GrupoId = id,
                UsuarioId = vencedor.UsuarioId,
                ValorPremio = valorPremio,
                DataSorteio = DateTime.Now
            };

            _context.Sorteio.Add(sorteio);

            // Remove os depósitos para zerar o grupo
            var depositos = _context.Deposito
                .Where(d => d.GrupoId == id)
                .ToList();

            _context.Deposito.RemoveRange(depositos);

            _context.SaveChanges();

            return RedirectToAction("ResultadoSorteio", new { id = sorteio.Id });
        }

        public IActionResult ResultadoSorteio(int id)
        {
            var sorteio = _context.Sorteio
                .Include(s => s.Usuario)
                .Include(s => s.Grupo)
                .FirstOrDefault(s => s.Id == id);

            if (sorteio == null)
            {
                return NotFound();
            }

            return View(sorteio);
        }

        public IActionResult RemoverParticipante(int id)
        {
            var participante = _context.ParticipanteGrupo
                .FirstOrDefault(p => p.Id == id);

            if (participante == null)
            {
                return NotFound();
            }

            int grupoId = participante.GrupoId;

            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuarioLogado = _context.Usuario
                .FirstOrDefault(u => u.Email == email);

            var grupo = _context.Grupo
                .FirstOrDefault(g => g.Id == grupoId);

            // Apenas o administrador pode remover participantes
            if (usuarioLogado == null || grupo == null || grupo.AdministradorId != usuarioLogado.Id)
            {
                return RedirectToAction("Detalhes", new { id = grupoId });
            }

            // O administrador não pode remover a si mesmo
            if (participante.UsuarioId == grupo.AdministradorId)
            {
                TempData["Erro"] = "O administrador não pode remover a si mesmo.";
                return RedirectToAction("Detalhes", new { id = grupoId });
            }

            // Busca os depósitos do participante nesse grupo
            var depositos = _context.Deposito
                .Where(d => d.GrupoId == grupoId &&
                            d.UsuarioId == participante.UsuarioId)
                .ToList();

            // Soma o valor depositado
            decimal valorDevolvido = depositos.Sum(d => d.Valor);

            // Devolve o dinheiro para o saldo do usuário
            var usuarioRemovido = _context.Usuario
                .FirstOrDefault(u => u.Id == participante.UsuarioId);

            if (usuarioRemovido != null)
            {
                usuarioRemovido.Saldo += valorDevolvido;
            }

            // Remove os depósitos do grupo
            _context.Deposito.RemoveRange(depositos);

            // Remove o participante
            _context.ParticipanteGrupo.Remove(participante);

            _context.SaveChanges();

            TempData["Sucesso"] = "Participante removido com sucesso.";

            return RedirectToAction("Detalhes", new { id = grupoId });
        }

        public IActionResult EditarGrupo(int id)
        {
            var grupo = _context.Grupo.FirstOrDefault(g => g.Id == id);

            if (grupo == null)
            {
                return NotFound();
            }

            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            if (usuario == null || grupo.AdministradorId != usuario.Id)
            {
                return RedirectToAction("Detalhes", new { id });
            }

            return View(grupo);
        }

        [HttpPost]
        public IActionResult EditarGrupo(Grupo grupo)
        {
            var grupoBanco = _context.Grupo
                .FirstOrDefault(g => g.Id == grupo.Id);

            if (grupoBanco == null)
            {
                return NotFound();
            }

            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario
                .FirstOrDefault(u => u.Email == email);

            if (usuario == null || grupoBanco.AdministradorId != usuario.Id)
            {
                return RedirectToAction("Detalhes", new { id = grupo.Id });
            }


            grupoBanco.Nome = grupo.Nome;
            grupoBanco.Descricao = grupo.Descricao;

            _context.SaveChanges();

            return RedirectToAction("Detalhes", new { id = grupo.Id });
        }

        public IActionResult SairGrupo(int id)
        {
            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            var grupo = _context.Grupo.FirstOrDefault(g => g.Id == id);

            if (grupo == null)
            {
                return NotFound();
            }

            var participante = _context.ParticipanteGrupo
                .FirstOrDefault(p => p.GrupoId == id && p.UsuarioId == usuario.Id);

            if (participante == null)
            {
                return RedirectToAction("GrupoTelaInicial");
            }

            // Checa se é o administrador
            if (grupo.AdministradorId == usuario.Id)
            {
                decimal total = _context.Deposito
                    .Where(d => d.GrupoId == id)
                    .Sum(d => d.Valor);

                if (total > 0)
                {
                    TempData["Erro"] = "O administrador não pode sair enquanto houver dinheiro no grupo.";

                    return RedirectToAction("Detalhes", new { id });
                }

                // Exclui o grupo
                _context.ParticipanteGrupo.Remove(participante);
                _context.Grupo.Remove(grupo);
                _context.SaveChanges();

                return RedirectToAction("GrupoTelaInicial");
            }

            // Participante comum
            _context.ParticipanteGrupo.Remove(participante);
            _context.SaveChanges();

            return RedirectToAction("GrupoTelaInicial");


        }

        [HttpGet]
        public IActionResult AdicionarSaldo()
        {
            return View();
        }


        [HttpPost]
        public IActionResult AdicionarSaldo(decimal valor)
        {
            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario
                .FirstOrDefault(u => u.Email == email);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Usuario");
            }


            if (valor <= 0)
            {
                TempData["Erro"] = "Informe um valor válido.";
                return RedirectToAction("AdicionarSaldo");
            }


            usuario.Saldo += valor;

            _context.Entry(usuario).Property(u => u.Saldo).IsModified = true;
            _context.SaveChanges();


            return RedirectToAction("Index", "Home");
        }
    }
}

