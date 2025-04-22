using Microsoft.AspNetCore.Mvc;
using TiendaVirtual.Data;
using TiendaVirtual.Models;

namespace TiendaVirtual.Controllers
{
    public class CatalogoController : Controller
    {
        // Mostrar todos los productos
        public IActionResult Index()
        {
            var productos = DBProducto.ObtenerProductos();
            return View(productos);
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
