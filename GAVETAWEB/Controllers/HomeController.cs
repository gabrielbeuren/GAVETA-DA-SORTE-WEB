using GAVETAWEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GAVETAWEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Contexto _context;
        public HomeController(ILogger<HomeController> logger, Contexto context)
        {
            _logger = logger;
            _context = context;
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UsuarioLogado") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            if (usuario != null)
            {
                ViewBag.Nome = usuario.Nome;
                ViewBag.Saldo = usuario.Saldo;
                ViewBag.FotoPerfil = string.IsNullOrEmpty(usuario.FotoPerfil)
                 ? "/images/avatar.png"
                 : usuario.FotoPerfil;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AdicionarSaldo()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdicionarSaldo(decimal valor)
        {
            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            usuario.Saldo += valor;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
           
        
    }
}
