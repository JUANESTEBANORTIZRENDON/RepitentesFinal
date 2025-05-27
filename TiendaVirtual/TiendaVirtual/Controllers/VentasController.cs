using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace TiendaVirtual.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ventas
        public async Task<IActionResult> Index(string busqueda, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Consulta base para obtener facturas con datos de usuario
            var query = _context.Facturas
                .Include(f => f.IdUsuarioNavigation)
                .AsQueryable();

            // Aplicar filtros si se proporcionaron
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                // Buscar por nombre de usuario o ID de factura
                query = query.Where(f => 
                    f.IdUsuarioNavigation.Nombre.Contains(busqueda) ||
                    f.IdUsuarioNavigation.Correo.Contains(busqueda) ||
                    f.IdFactura.ToString() == busqueda);
            }

            // Filtrar por fecha inicial si se proporcionó
            if (fechaDesde.HasValue)
            {
                // Ajustar para incluir todo el día de inicio
                var fechaDesdeAjustada = fechaDesde.Value.Date;
                query = query.Where(f => f.Fecha >= fechaDesdeAjustada);
            }

            // Filtrar por fecha final si se proporcionó
            if (fechaHasta.HasValue)
            {
                // Ajustar para incluir todo el día final
                var fechaHastaAjustada = fechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(f => f.Fecha <= fechaHastaAjustada);
            }

            // Ordenar por fecha descendente y ejecutar la consulta
            var facturas = await query
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();

            // Guardar los parámetros de búsqueda para mantenerlos en la vista
            ViewBag.Busqueda = busqueda;
            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");

            return View(facturas);
        }

        // GET: Ventas/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener la factura con todos sus detalles y datos relacionados
            var factura = await _context.Facturas
                .Include(f => f.IdUsuarioNavigation)
                .Include(f => f.FacturaDetalles)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(f => f.IdFactura == id);

            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // GET: Ventas/HistorialUsuario/5
        public async Task<IActionResult> HistorialUsuario(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener el usuario
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Obtener todas las facturas del usuario
            var facturas = await _context.Facturas
                .Where(f => f.IdUsuario == id)
                .Include(f => f.FacturaDetalles)
                    .ThenInclude(d => d.IdProductoNavigation)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();

            ViewBag.Usuario = usuario;
            return View(facturas);
        }

        // GET: Ventas/DescargarFactura/5
        public async Task<IActionResult> DescargarFactura(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null || string.IsNullOrEmpty(factura.RutaPdf))
            {
                return NotFound("Factura no encontrada o sin PDF.");
            }

            var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", factura.RutaPdf);
            if (!System.IO.File.Exists(path))
            {
                return NotFound("Archivo PDF no encontrado.");
            }

            var pdfBytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(pdfBytes, "application/pdf", $"factura_{factura.IdFactura}.pdf");
        }
    }
}
