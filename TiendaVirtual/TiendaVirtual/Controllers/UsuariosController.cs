using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Models;
using TiendaVirtual.Data;

namespace TiendaVirtual.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios.Include(u => u.IdRolNavigation).ToListAsync();
            return View(usuarios);
        }

        // GET: Usuarios/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // GET: Usuarios/Crear
        public IActionResult Crear()
        {
            ViewBag.Roles = _context.Rols.ToList();
            return View();
        }

        // POST: Usuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                usuario.Clave = BCrypt.Net.BCrypt.HashPassword(usuario.Clave);
                usuario.Confirmado = true;
                usuario.TokenConfirmacion = null;
                usuario.TokenRecuperacion = null;

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = _context.Rols.ToList();
            return View(usuario);
        }

        // GET: Usuarios/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewBag.Roles = _context.Rols.ToList();
            return View(usuario);
        }

        // POST: Usuarios/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario) return NotFound();

            var exito = await new DBUsuario(_context).ActualizarUsuarioAsync(usuario);

            if (!exito)
            {
                ViewBag.MensajeSinCambios = "No se realizaron cambios porque los datos no fueron modificados.";
                ViewBag.Roles = _context.Rols.ToList();
                return View(usuario);
            }

            TempData["mensaje"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }





        // GET: Usuarios/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuarios/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExiste(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }


        // GET: Usuarios/CambiarClave/5
        public async Task<IActionResult> CambiarClave(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuarios/CambiarClave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarClave(int id, string nuevaClave)
        {
            if (string.IsNullOrWhiteSpace(nuevaClave))
            {
                TempData["error"] = "La nueva contraseña no puede estar vacía.";
                return RedirectToAction("CambiarClave", new { id });
            }

            var exito = await new DBUsuario(_context).CambiarClaveAsync(id, nuevaClave);
            if (exito)
            {
                TempData["mensaje"] = "Contraseña actualizada con éxito.";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "No se pudo actualizar la contraseña.";
            return RedirectToAction("CambiarClave", new { id });
        }

        // GET: Usuarios/CambiarRol/5
        public async Task<IActionResult> CambiarRol(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewBag.Roles = _context.Rols.ToList();
            return View(usuario);
        }

        // POST: Usuarios/CambiarRol/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(int id, int nuevoRolId)
        {
            var exito = await new DBUsuario(_context).CambiarRolAsync(id, nuevoRolId);
            if (exito)
            {
                TempData["mensaje"] = "Rol actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "No se realizaron cambios en el rol.";
            return RedirectToAction("CambiarRol", new { id });
        }

    }
}

