using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TiendaVirtual.Models;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int cantidadProductos = 8)
        {
            // Obtener productos destacados para mostrar en la página de inicio
            // Solo productos activos
            var productosDestacados = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Where(p => p.Activo == true || p.Activo == null)
                .OrderByDescending(p => p.IdProducto) // Más recientes primero
                .Take(cantidadProductos)
                .ToList();

            return View(productosDestacados);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Ventas()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Admin()
        {
            return View();
        }

    }
}