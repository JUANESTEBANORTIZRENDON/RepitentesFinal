using Microsoft.AspNetCore.Mvc;
using TiendaVirtual.Data;
using TiendaVirtual.Models;
using System.Threading.Tasks;

namespace TiendaVirtual.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly DBProducto _dbProducto;

        public CatalogoController(DBProducto dbProducto)
        {
            _dbProducto = dbProducto;
        }

        public async Task<IActionResult> Index(string busqueda, string orden, int pagina = 1, int tamanoPagina = 8)
        {
            // Usar el método de DBProducto para obtener productos filtrados
            var resultado = await _dbProducto.ObtenerProductosCatalogoAsync(busqueda, orden, pagina, tamanoPagina);
            var productos = resultado.productos;
            int totalProductos = resultado.total;

            // Cargar las categorías
            var categorias = await _dbProducto.ObtenerCategoriasAsync();
            ViewBag.Categorias = categorias.ToDictionary(c => c.IdCategoria, c => c.Nombre);

            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalProductos / tamanoPagina);
            ViewBag.PaginaActual = pagina;
            ViewBag.Busqueda = busqueda;
            ViewBag.Orden = orden;

            return View(productos);
        }
        
        [HttpGet]
        public async Task<IActionResult> VerificarStock(int idProducto, int cantidad)
        {
            // Verificar si hay suficiente stock
            bool hayStock = await _dbProducto.VerificarStockDisponibleAsync(idProducto, cantidad);
            if (!hayStock)
            {
                // Si no hay suficiente stock, retornar información sobre el stock disponible
                var producto = await _dbProducto.ObtenerPorIdAsync(idProducto);
                return Json(new { exito = false, mensaje = $"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles.", stockActual = producto.Stock });
            }
            
            return Json(new { exito = true });
        }

    }
}


