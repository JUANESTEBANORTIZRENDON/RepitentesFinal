using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using TiendaVirtual.Models;
using TiendaVirtual.Data;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;


namespace TiendaVirtual.Controllers
{
    public class CuentaController : Controller
    {
        private readonly DBUsuario _dbUsuario;
        private readonly ServidorSettings _servidorSettings;

        public CuentaController(DBUsuario dbUsuario, IOptions<ServidorSettings> servidorSettings)
        {
            _dbUsuario = dbUsuario;
            _servidorSettings = servidorSettings.Value;
        }

        // Registro de nuevo usuario
        public IActionResult Registrar() => View();

        [HttpPost]
        public async Task<IActionResult> Registrar(Usuario usuario)
        {
            // Validar si el correo ya existe
            bool correoExiste = await _dbUsuario.ExisteCorreoAsync(usuario.Correo);

            if (correoExiste)
            {
                ModelState.AddModelError("Correo", "Este correo ya está registrado.");
                return View(usuario); // Vuelve a la vista de registro con el mensaje de error
            }

            await _dbUsuario.RegistrarUsuarioAsync(usuario);
            // Construcción de la URL de confirmación usando la IP y puerto del appsettings.json
            string urlConfirmacion = $"http://{_servidorSettings.IpLocal}:{_servidorSettings.Puerto}/Cuenta/Confirmar?token={usuario.TokenConfirmacion}";
            await EnviarCorreo(usuario.Correo, usuario.Nombre, "Confirma tu cuenta", urlConfirmacion, "ConfirmacionCuenta.html");


            TempData["mensaje"] = "Verifica tu correo electrónico para confirmar tu cuenta.";
            // Aquí la redirección a la vista RegistroExitoso
            return View("RegistroExitoso");
        }


        // Confirmar la cuenta vía token
        public async Task<IActionResult> Confirmar(string token)
        {
            bool confirmado = await _dbUsuario.ConfirmarCuentaAsync(token);

            if (confirmado)
            {
                return View("Confirmar"); // ← Aquí cargas la vista Confirmar.cshtml
            }
            else
            {
                TempData["mensaje"] = "Token inválido o expirado.";
                return RedirectToAction("Login");
            }
        }


        // Página de inicio de sesión
        public IActionResult Login() => View();


        [HttpPost]
        public async Task<IActionResult> Login(string correo, string clave)
        {
            // Verifica el usuario con la base de datos
            var usuario = await _dbUsuario.LoginAsync(correo, clave);

            if (usuario == null)
            {
                ViewBag.Mensaje = "Credenciales inválidas o cuenta no confirmada.";
                return View();
            }

            // Obtener el nombre del rol por el IdRol del usuario
            string nombreRol = await _dbUsuario.ObtenerNombreRolAsync((int)usuario.IdRol);

            // Crear los claims para la cookie
            var claims = new List<Claim>
            {
             new Claim(ClaimTypes.Name, usuario.Nombre),
             new Claim(ClaimTypes.Role, nombreRol), // Aquí va el nombre del rol (Administrador o Cliente)
             new Claim("correo", usuario.Correo)


            };

            

            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));

            // Redireccionar según el rol del usuario
            if (nombreRol == "Administrador")
            {
                return RedirectToAction("Admin", "Home");

            }
            else
            {
                return RedirectToAction("Index", "Home"); // Vista normal del cliente
            }
        }


        // Cerrar sesión
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login");
        }


        // Página solicitar recuperación contraseña
        public IActionResult Recuperar() => View();

        [HttpPost]
        public async Task<IActionResult> Recuperar(string correo)
        {
            bool generado = await _dbUsuario.GenerarTokenRecuperacionAsync(correo);

            if (generado)
            {
                // Aquí puedes volver a consultar el usuario solo para obtener los datos que necesitas
                var usuario = await _dbUsuario.ObtenerUsuarioPorCorreoAsync(correo);

                // Construcción de la URL de recuperación usando la IP y puerto del appsettings.json
                string urlReset = $"http://{_servidorSettings.IpLocal}:{_servidorSettings.Puerto}/Cuenta/Restablecer?token={usuario.TokenRecuperacion}";
                await EnviarCorreo(usuario.Correo, usuario.Nombre, "Restablecer contraseña", urlReset, "RecuperacionPassword.html");

            }
            return View("RecuperacionEnviada");
        }


        // Página restablecer contraseña
        public IActionResult Restablecer(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Restablecer(string token, string nuevaClave)
        {
            // Validar que no haya campos vacíos desde el inicio
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(nuevaClave))
            {
                TempData["mensaje"] = "Debes ingresar una nueva contraseña.";
                ViewBag.Token = token;
                return View("Restablecer");
            }

            try
            {
                // Buscar el usuario con el token (verifica expiración también)
                var usuario = await _dbUsuario.ObtenerUsuarioPorTokenAsync(token);

                if (usuario == null)
                {
                    TempData["mensaje"] = "Token inválido, expirado o inexistente.";
                    ViewBag.Token = token;
                    return View("Restablecer");
                }

                // Validar que la nueva contraseña NO sea igual a la actual
                bool mismaClave = await _dbUsuario.ClaveEsIgualAsync(usuario.IdUsuario, nuevaClave);

                if (mismaClave)
                {
                    TempData["mensaje"] = "La nueva contraseña no puede ser igual a la anterior.";
                    ViewBag.Token = token; // Mantienes el token para que no se pierda en la vista
                    return View("Restablecer");
                }

                // Si es diferente, cambiamos la contraseña
                bool restablecido = await _dbUsuario.RestablecerPasswordAsync(token, nuevaClave);

                if (restablecido)
                {
                    TempData["mensaje"] = "Contraseña restablecida exitosamente.";
                    return View("RestablecerConfirmacion"); // Vista de éxito
                }
                else
                {
                    TempData["mensaje"] = "Ocurrió un error al restablecer la contraseña.";
                    ViewBag.Token = token;
                    return View("Restablecer");
                }
            }
            catch (Exception ex)
            {
                TempData["mensaje"] = $"Error: {ex.Message}";
                ViewBag.Token = token;
                return View("Restablecer");
            }
        }


        // Método para enviar correos usando plantilla HTML
        private async Task EnviarCorreo(string correo, string nombre, string asunto, string url, string plantilla)
        {
            var rutaPlantilla = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/EmailTemplates", plantilla);
            var cuerpo = await System.IO.File.ReadAllTextAsync(rutaPlantilla);
            cuerpo = string.Format(cuerpo, nombre, url);

            //definicion del cuerpo del correo 
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Tienda Virtual", "tu_correo@gmail.com"));
            mensaje.To.Add(new MailboxAddress(nombre, correo));
            mensaje.Subject = asunto;
            mensaje.Body = new BodyBuilder { HtmlBody = cuerpo }.ToMessageBody();

            using var cliente = new SmtpClient();
            await cliente.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await cliente.AuthenticateAsync("cazawikis42@gmail.com", "mjie jgoz gbal mtmi");
            await cliente.SendAsync(mensaje);
            await cliente.DisconnectAsync(true);
        }
    }
}
