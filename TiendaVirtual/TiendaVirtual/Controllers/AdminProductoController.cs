using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Data;
using TiendaVirtual.Models;
using System.Threading.Tasks;

namespace TiendaVirtual.Controllers
{
    public class AdminProductoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DBProducto _dbProducto;
        private readonly DBCategoria _dbCategoria;

        public AdminProductoController(ApplicationDbContext context)
        {
            _context = context;
            _dbProducto = new DBProducto(_context);
            _dbCategoria = new DBCategoria(_context);
        }

        public async Task<IActionResult> Index(string busqueda, string orden, bool? filtroActivo = null)
        {
            // Obtener todos los productos, activos e inactivos
            var productos = await _dbProducto.ObtenerProductosAsync(filtroActivo.GetValueOrDefault());

            // Obtener categorías
            var categorias = await _dbCategoria.ObtenerCategoriasAsync();

            // Diccionario para mostrar nombre de la categoría desde su ID
            ViewBag.Categorias = categorias.ToDictionary(c => c.IdCategoria, c => c.Nombre);

            // Filtro de búsqueda
            if (!string.IsNullOrEmpty(busqueda))
            {
                productos = productos.Where(p =>
                    p.Nombre?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) == true ||
                    p.Marca?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) == true ||
                    p.CodigoProducto?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) == true
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

            ViewBag.FiltroActivo = filtroActivo;
            ViewBag.Busqueda = busqueda;
            ViewBag.Orden = orden;
            return View(productos);
        }

        public async Task<IActionResult> Crear()
        {
            ViewBag.Categorias = await _dbCategoria.ObtenerCategoriasAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Producto producto)
        {
            ViewBag.Categorias = await _dbCategoria.ObtenerCategoriasAsync();
            if (ModelState.IsValid)
            {
                // Validar si el producto ya existe antes de insertar
                if (await _dbProducto.ProductoExisteAsync(producto.CodigoProducto ?? "", producto.Nombre ?? ""))
                {
                    ModelState.AddModelError("", "Ya existe un producto con el mismo código o nombre.");
                    return View(producto);
                }

                try
                {
                    await _dbProducto.InsertarProductoAsync(producto);
                    TempData["mensaje"] = "Producto añadido con éxito.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ocurrió un error al añadir el producto: {ex.Message}");
                }
            }
            return View(producto);
        }


        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _dbProducto.ObtenerPorIdAsync(id);
            if (producto == null) return NotFound();

            ViewBag.Categorias = await _dbCategoria.ObtenerCategoriasAsync();
            return View(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Producto producto)
        {
            ViewBag.Categorias = await _dbCategoria.ObtenerCategoriasAsync();
            if (ModelState.IsValid)
            {
                try
                {
                    await _dbProducto.ActualizarProductoAsync(producto);
                    TempData["mensaje"] = "Producto actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar el producto: {ex.Message}");
                }
            }
            return View(producto);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarSeleccionados(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["error"] = "Debes seleccionar al menos un producto para desactivar.";
                return RedirectToAction("Index");
            }

            try
            {
                foreach (var id in ids)
                {
                    await _dbProducto.CambiarEstadoProductoAsync(id, false);
                }

                TempData["mensaje"] = "Productos desactivados correctamente.";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error al desactivar productos: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, bool estado)
        {
            try
            {
                await _dbProducto.CambiarEstadoProductoAsync(id, estado);
                TempData["mensaje"] = $"Producto {(estado ? "activado" : "desactivado")} correctamente.";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error al {(estado ? "activar" : "desactivar")} el producto: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ActivarSeleccionados(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                TempData["error"] = "Debes seleccionar al menos un producto para activar.";
                return RedirectToAction("Index");
            }

            try
            {
                foreach (var id in ids)
                {
                    await _dbProducto.CambiarEstadoProductoAsync(id, true);
                }

                TempData["mensaje"] = "Productos activados correctamente.";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error al activar productos: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }
    }
}

