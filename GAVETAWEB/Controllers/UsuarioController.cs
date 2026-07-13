

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

            TempData["Sucesso"] = "Cadastro realizado com sucesso! Faça login.";

            return RedirectToAction("Login");
            
        }
        public IActionResult EditarCadastro()
        {
            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
            {
                return RedirectToAction("Login");
            }

            return View(usuario);
        }

        [HttpPost]
        public IActionResult EditarCadastro(Usuario usuario)
        {
            ModelState.Remove("Senha");
            ModelState.Remove("Tipo");

            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuarioBanco = _context.Usuario
                .FirstOrDefault(u => u.Email == email);

            if (usuarioBanco == null)
            {
                return RedirectToAction("Login");
            }

            usuarioBanco.Nome = usuario.Nome;
            usuarioBanco.Email = usuario.Email;
            usuarioBanco.CPF = usuario.CPF;
            usuarioBanco.DataNascimento = usuario.DataNascimento;
            usuarioBanco.Sexo = usuario.Sexo;

            _context.SaveChanges();

            // Atualiza a sessão caso o e-mail tenha sido alterado
            HttpContext.Session.SetString("UsuarioLogado", usuarioBanco.Email);

            TempData["Sucesso"] = "Cadastro atualizado com sucesso!";

            return RedirectToAction("Login", "Usuario");
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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Usuario");
        }

        public IActionResult AlterarFoto()
        {
            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario
                .FirstOrDefault(u => u.Email == email);

            if (usuario == null)
            {
                return RedirectToAction("Login");
            }

            return View(usuario);
        }

        [HttpPost]
        public IActionResult AlterarFoto(IFormFile foto)
        {
            string email = HttpContext.Session.GetString("UsuarioLogado");

            var usuario = _context.Usuario
                .FirstOrDefault(u => u.Email == email);


            if (usuario == null)
            {
                return RedirectToAction("Login");
            }


            if (foto != null && foto.Length > 0)
            {
                // Pasta onde as imagens serão salvas
                string pasta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/perfis"
                );


                // Cria a pasta se não existir
                if (!Directory.Exists(pasta))
                {
                    Directory.CreateDirectory(pasta);
                }


                // Nome único para evitar conflito
                string nomeArquivo = Guid.NewGuid().ToString()
                    + Path.GetExtension(foto.FileName);


                string caminhoCompleto = Path.Combine(
                    pasta,
                    nomeArquivo
                );


                // Salva o arquivo
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    foto.CopyTo(stream);
                }


                // Salva o caminho no banco
                usuario.FotoPerfil = "/uploads/perfis/" + nomeArquivo;


                _context.SaveChanges();
            }


            
            return RedirectToAction("Index", "Home");
        }


    }
}
