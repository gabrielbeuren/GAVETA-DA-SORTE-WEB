

using GAVETAWEB.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
namespace GAVETAWEB.Controllers
{
    public class UsuarioController : Controller
    {

        private readonly Contexto _context;

        public UsuarioController(Contexto context)
        {
            _context = context;
        }
        public IActionResult Cadastro()
        {
            
            return View();
        }

        [HttpPost]
        public IActionResult Cadastro(Usuario usuario)
        {
            usuario.Tipo = "Usuario";
            usuario.DataCadastro = DateTime.Now;
            _context.Usuario.Add(usuario);
            _context.SaveChanges();
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string senha)
        {
            var usuarioBanco = _context.Usuario.FirstOrDefault(u =>
             u.Email == email &&
             u.Senha == senha);

            if (usuarioBanco != null)
            {
                HttpContext.Session.SetString("UsuarioLogado", usuarioBanco.Email);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Mensagem = "E-mail ou senha inválidos.";
            return View();
        }
    }
}
