using Microsoft.AspNetCore.Mvc;
using TiendaVirtual.Data;
using TiendaVirtual.Models;

namespace TiendaVirtual.Controllers
{
    public class HistorialController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistorialController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pagina = 1, int tamanoPagina = 5)
        {
            int idUsuario = ObtenerIdUsuario();
            var db = new DBHistorial(_context);

            var historialCompleto = await db.ObtenerHistorialPorUsuarioAsync(idUsuario);

            int totalRegistros = historialCompleto.Count;
            var historialPaginado = historialCompleto
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToList();

            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
            ViewBag.PaginaActual = pagina;

            return View(historialPaginado);
        }


        private int ObtenerIdUsuario()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return int.Parse(claim?.Value ?? "0");
        }

    }
}

