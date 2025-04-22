using Microsoft.AspNetCore.Mvc;
using TiendaVirtual.Data;
using TiendaVirtual.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace TiendaVirtual.Controllers
{
    public class CuentaController : Controller
    {
        private readonly DBUsuario _db;

        public CuentaController(TiendaDbContext context)
        {
            _db = new DBUsuario(context);
        }

        // Vista de login
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string correo, string clave)
        {
            string claveHash = Hashear(clave);
            var user = await _db.LoginAsync(correo, claveHash);

            if (user == null)
            {
                ViewBag.Mensaje = "Credenciales inválidas.";
                return View();
            }

            string nombreRol = await _db.ObtenerNombreRol(user.IdRol);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Role, nombreRol),
                new Claim("correo", user.Correo)
            };

            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));

            return RedirectToAction("Index", "Home");
        }

        // Vista de registro
        public IActionResult Registro() => View();

        [HttpPost]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            //valida si correo a crear ya existe 
            if (_db.ExisteCorreo(usuario.Correo))
            {
                ModelState.AddModelError("Correo", "Este correo ya está registrado.");
                return View(usuario);
            }

            usuario.Clave = Hashear(usuario.Clave);
            usuario.IdRol = 2; // Cliente
            await _db.RegistrarClienteAsync(usuario);
            return RedirectToAction("Login");
        }

        // Vista de cambio de contraseña
        public IActionResult CambiarClave() => View();

        [HttpPost]
        public async Task<IActionResult> CambiarClave(string correo, string nuevaClave)
        {
            var usuario = await _db.BuscarPorCorreoAsync(correo);

            if (usuario == null)
            {
                ViewBag.Mensaje = "Correo no registrado.";
                return View();
            }

            string claveHash = Hashear(nuevaClave);
            await _db.CambiarClaveAsync(usuario, claveHash);
            ViewBag.Mensaje = "Contraseña actualizada con éxito.";
            return View();
        }

        // Cerrar sesión
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        private string Hashear(string texto)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(texto);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}

