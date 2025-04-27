using Microsoft.AspNetCore.Mvc;
using TiendaVirtual.Data;
using TiendaVirtual.Models;

namespace TiendaVirtual.Controllers
{
    public class CatalogoController : Controller
    {
        // Mostrar todos los productos
        public IActionResult Index(string busqueda, string orden, int pagina = 1, int tamañoPagina = 8)
        {
            var productos = DBProducto.ObtenerProductos();

            if (!string.IsNullOrEmpty(busqueda))
            {
                productos = productos.Where(p =>
                    p.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    p.Marca.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    p.CodigoProducto.Contains(busqueda, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            productos = orden switch
            {
                "precio_asc" => productos.OrderBy(p => p.PrecioUnitario).ToList(),
                "precio_desc" => productos.OrderByDescending(p => p.PrecioUnitario).ToList(),
                "stock_asc" => productos.OrderBy(p => p.Stock).ToList(),
                "stock_desc" => productos.OrderByDescending(p => p.Stock).ToList(),
                _ => productos
            };

            int totalProductos = productos.Count;
            var productosPaginados = productos.Skip((pagina - 1) * tamañoPagina).Take(tamañoPagina).ToList();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalProductos / tamañoPagina);
            ViewBag.Busqueda = busqueda;
            ViewBag.Orden = orden;

            return View(productosPaginados);
        }


        // Mostrar formulario para crear producto (GET)
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // Registrar nuevo producto (POST)
        [HttpPost]
        public IActionResult Crear(Producto producto)
        {
            if (ModelState.IsValid)
            {
                bool insertado = DBProducto.InsertarProducto(producto);
                if (insertado)
                {
                    TempData["mensaje"] = "Producto registrado correctamente.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Hubo un error al registrar el producto.";
                }
            }
            return View(producto);
        }
    }
}
