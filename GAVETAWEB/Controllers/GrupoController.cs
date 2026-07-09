using GAVETAWEB.Models;
using Microsoft.AspNetCore.Mvc;

namespace GAVETAWEB.Controllers
{
    public class GrupoController : Controller
    {

        private readonly Contexto _context;

        public GrupoController(Contexto context)
        {
            _context = context;
        }

        public IActionResult GrupoTelaInicial()
        {
            var grupos = _context.Grupo.ToList();

            return View(grupos);
        }

        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastro(Grupo grupo)
        {
            grupo.DataCriacao = DateTime.Now;

            _context.Grupo.Add(grupo);
            _context.SaveChanges();

            return RedirectToAction("GrupoTelaInicial");
        }
    }
}

