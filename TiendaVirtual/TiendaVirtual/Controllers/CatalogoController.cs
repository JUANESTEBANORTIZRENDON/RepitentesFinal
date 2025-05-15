using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Models;

namespace TiendaVirtual.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string busqueda, string orden, int pagina = 1, int tamanoPagina = 8)
        {
            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    p.Marca.Contains(busqueda) ||
                    p.CodigoProducto.Contains(busqueda));
            }

            switch (orden)
            {
                case "precio_asc":
                    query = query.OrderBy(p => p.PrecioUnitario);
                    break;
                case "precio_desc":
                    query = query.OrderByDescending(p => p.PrecioUnitario);
                    break;
                case "stock_asc":
                    query = query.OrderBy(p => p.Stock);
                    break;
                case "stock_desc":
                    query = query.OrderByDescending(p => p.Stock);
                    break;
            }

            int totalProductos = query.Count();
            var productos = query
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToList();

            // ✅ Cargar las categorías y pasarlas a la vista como diccionario
            var categorias = _context.Categoria.ToDictionary(c => c.IdCategoria, c => c.Nombre);
            ViewBag.Categorias = categorias;

            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalProductos / tamanoPagina);
            ViewBag.PaginaActual = pagina;
            ViewBag.Busqueda = busqueda;
            ViewBag.Orden = orden;

            return View(productos);
        }

    }
}


