using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Models;
using PdfSharpCore.Pdf;
//using HtmlRendererCore.PdfSharpCore;
using System.Text;
using System.Security.Claims;
using TiendaVirtual.Data;
using Microsoft.AspNetCore.Authorization;

namespace TiendaVirtual.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CarritoController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int ObtenerIdUsuario()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(id ?? "0");
        }

        public async Task<IActionResult> Index()
        {
            int idUsuario = ObtenerIdUsuario();

            var carrito = await _context.Carritos
                .Include(c => c.CarritoProductos)
                    .ThenInclude(cp => cp.IdProductoNavigation)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito == null) return View(new List<CarritoProducto>());

            // Calcular total
            decimal total = carrito.CarritoProductos.Sum(cp => cp.Cantidad * cp.PrecioUnitario);
            ViewBag.Total = total;

            return View(carrito.CarritoProductos.ToList());
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Agregar(int idProducto, int cantidad)
        {
            int idUsuario = ObtenerIdUsuario();

            if (idUsuario == 0)
            {
                TempData["error"] = "Error al identificar usuario autenticado.";
                return RedirectToAction("Login", "Cuenta");
            }

            var carrito = await _context.Carritos
                .Include(c => c.CarritoProductos)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito == null)
            {
                carrito = new Carrito { IdUsuario = idUsuario, FechaActualizacion = DateTime.Now };
                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            var existente = carrito.CarritoProductos.FirstOrDefault(cp => cp.IdProducto == idProducto);
            var unitario = await _context.Productos.FindAsync(idProducto);

            if (existente != null)
            {
                existente.Cantidad += cantidad;
            }
            else
            {
                carrito.CarritoProductos.Add(new CarritoProducto
                {
                    IdProducto = idProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = unitario.PrecioUnitario
                });
            }

            carrito.FechaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["mensaje"] = "Producto agregado al carrito.";
            return RedirectToAction("Index", "Catalogo");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int idProducto)
        {
            int idUsuario = ObtenerIdUsuario();

            var carrito = await _context.Carritos
                .Include(c => c.CarritoProductos)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito != null)
            {
                var producto = carrito.CarritoProductos.FirstOrDefault(cp => cp.IdProducto == idProducto);
                if (producto != null)
                {
                    _context.CarritoProductos.Remove(producto);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCantidad(List<CarritoProducto> productos)
        {
            int idUsuario = ObtenerIdUsuario();

            var carrito = await _context.Carritos
                .Include(c => c.CarritoProductos)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito != null)
            {
                foreach (var item in productos)
                {
                    var existente = carrito.CarritoProductos.FirstOrDefault(cp => cp.IdProducto == item.IdProducto);
                    if (existente != null)
                    {
                        existente.Cantidad = item.Cantidad;
                    }
                }

                carrito.FechaActualizacion = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Comprar()
        {
            var idUsuario = ObtenerIdUsuario();
            var dbFactura = new DBFactura(_context, _env);

            try
            {
                var factura = await dbFactura.GenerarFacturaAsync(idUsuario);
                if (factura == null)
                {
                    TempData["error"] = "Tu carrito está vacío.";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("FacturaGenerada", new { id = factura.IdFactura });
            }
            catch (InvalidOperationException ex)
            {
                // ⚠️ Mostramos mensaje con productos agotados
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }





        public async Task<IActionResult> FacturaGenerada(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.IdUsuarioNavigation) // Esto es útil si luego quieres mostrar el nombre del usuario, etc.
                .FirstOrDefaultAsync(f => f.IdFactura == id);

            if (factura == null) return NotFound();

            var detalles = await _context.FacturaDetalles
                .Include(d => d.IdProductoNavigation) // Esto es esencial para mostrar el nombre del producto en la vista
                .Where(d => d.IdFactura == id)
                .ToListAsync();

            ViewBag.Detalles = detalles;
            return View(factura);
        }


        private async Task<string> GenerarHtmlFactura(int idFactura)
        {
            var factura = await _context.Facturas
                .Include(f => f.IdUsuarioNavigation)
                .Include(f => f.FacturaDetalles)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(f => f.IdFactura == idFactura);

            var sb = new StringBuilder();
            sb.AppendLine("<h1>Factura de Compra</h1>");
            sb.AppendLine($"<p>Cliente: {factura.IdUsuarioNavigation.Nombre}</p>");
            sb.AppendLine($"<p>Fecha: {factura.Fecha}</p>");
            sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0'><thead><tr><th>Producto</th><th>Precio</th><th>Cantidad</th><th>Total</th></tr></thead><tbody>");
            foreach (var item in factura.FacturaDetalles)
            {
                sb.AppendLine($"<tr><td>{item.IdProductoNavigation.Nombre}</td><td>{item.PrecioUnitario:C}</td><td>{item.Cantidad}</td><td>{item.Cantidad * item.PrecioUnitario:C}</td></tr>");
            }
            sb.AppendLine("</tbody></table>");
            sb.AppendLine($"<h3>Total: {factura.Total:C}</h3>");
            return sb.ToString();
        }

        public async Task<IActionResult> DescargarPDF(int id)
        {
            var factura = await _context.Facturas.FindAsync(id);

            if (factura == null || string.IsNullOrEmpty(factura.RutaPdf))
            {
                return NotFound("Factura no encontrada o sin PDF.");
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", factura.RutaPdf);

            if (!System.IO.File.Exists(path))
            {
                return NotFound("Archivo PDF no encontrado.");
            }

            var pdfBytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(pdfBytes, "application/pdf", $"factura_{factura.IdFactura}.pdf");
        }

    }
}

