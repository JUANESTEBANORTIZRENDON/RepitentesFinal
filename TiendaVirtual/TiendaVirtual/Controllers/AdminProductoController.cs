using Microsoft.AspNetCore.Mvc;
using TiendaVirtual.Data;
using TiendaVirtual.Models;

namespace TiendaVirtual.Controllers
{
    public class AdminProductoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminProductoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string busqueda, string orden)
        {
            var productos = DBProducto.ObtenerProductos();

            // ✅ Se crea instancia de DBCategoria para llamar método NO estático
            var categorias = new DBCategoria(_context).ObtenerCategorias();

            // ✅ Diccionario para mostrar nombre de la categoría desde su ID
            ViewBag.Categorias = categorias.ToDictionary(c => c.IdCategoria, c => c.Nombre);

            // Filtro de búsqueda
            if (!string.IsNullOrEmpty(busqueda))
            {
                productos = productos.Where(p =>
                    p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    p.Marca.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    p.CodigoProducto.Contains(busqueda, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Ordenamiento
            productos = orden switch
            {
                "precio_asc" => productos.OrderBy(p => p.PrecioUnitario).ToList(),
                "precio_desc" => productos.OrderByDescending(p => p.PrecioUnitario).ToList(),
                "stock_asc" => productos.OrderBy(p => p.Stock).ToList(),
                "stock_desc" => productos.OrderByDescending(p => p.Stock).ToList(),
                _ => productos
            };

            return View(productos);
        }

        public IActionResult Crear()
        {
            ViewBag.Categorias = DBProducto.ObtenerCategorias();
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Producto producto)
        {
            ViewBag.Categorias = DBProducto.ObtenerCategorias();
            if (ModelState.IsValid)
            {
                if (DBProducto.InsertarProducto(producto))
                {
                    TempData["mensaje"] = "Producto añadido con éxito.";
                    return RedirectToAction("Index");
                }
            }
            return View(producto);
        }

        public IActionResult Editar(int id)
        {
            var producto = DBProducto.ObtenerPorId(id);
            if (producto == null) return NotFound();

            ViewBag.Categorias = DBProducto.ObtenerCategorias();
            return View(producto);
        }

        [HttpPost]
        public IActionResult Editar(Producto producto)
        {
            ViewBag.Categorias = DBProducto.ObtenerCategorias();
            if (ModelState.IsValid)
            {
                if (DBProducto.ActualizarProducto(producto))
                {
                    TempData["mensaje"] = "Producto actualizado correctamente.";
                    return RedirectToAction("Index");
                }
            }
            return View(producto);
        }

        [HttpPost]
        public IActionResult EliminarSeleccionados(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["error"] = "Debes seleccionar al menos un producto para eliminar.";
                return RedirectToAction("Index");
            }

            foreach (var id in ids)
            {
                DBProducto.EliminarProducto(id);
            }

            TempData["mensaje"] = "Productos eliminados correctamente.";
            return RedirectToAction("Index");
        }
    }
}

