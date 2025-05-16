using TiendaVirtual.Models;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Pdf;
//using HtmlRendererCore.PdfSharpCore;
using System.Text;
using PdfSharpCore;
using VetCV.HtmlRendererCore.PdfSharpCore;

namespace TiendaVirtual.Data
{
    public class DBFactura
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DBFactura(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<Factura?> GenerarFacturaAsync(int idUsuario)
        {
            var carrito = await _context.Carritos
                .Include(c => c.CarritoProductos)
                .ThenInclude(cp => cp.IdProductoNavigation)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito == null || carrito.CarritoProductos.Count == 0)
                return null;

            // ✅ Verificar stock de cada producto
            var productosSinStock = carrito.CarritoProductos
                .Where(cp => cp.Cantidad > cp.IdProductoNavigation.Stock)
                .Select(cp => cp.IdProductoNavigation.Nombre)
                .ToList();

            if (productosSinStock.Any())
            {
                // Lanzamos excepción controlada para manejarla en el controlador
                throw new InvalidOperationException(
                    "Stock insuficiente para: " + string.Join(", ", productosSinStock));
            }

            // ✅ Descontar stock
            foreach (var cp in carrito.CarritoProductos)
            {
                cp.IdProductoNavigation.Stock -= cp.Cantidad;
            }

            // ✅ Total de la compra
            decimal total = carrito.CarritoProductos.Sum(cp => cp.Cantidad * cp.PrecioUnitario);

            var factura = new Factura
            {
                IdUsuario = idUsuario,
                Fecha = DateTime.Now,
                Total = total
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            foreach (var cp in carrito.CarritoProductos)
            {
                var detalle = new FacturaDetalle
                {
                    IdFactura = factura.IdFactura,
                    IdProducto = cp.IdProducto,
                    Cantidad = cp.Cantidad,
                    PrecioUnitario = cp.PrecioUnitario
                };
                _context.FacturaDetalles.Add(detalle);
            }

            _context.HistorialCompras.Add(new HistorialCompra
            {
                IdUsuario = idUsuario,
                IdFactura = factura.IdFactura,
                Fecha = DateTime.Now
            });

            // ✅ Vaciar carrito
            _context.CarritoProductos.RemoveRange(carrito.CarritoProductos);
            await _context.SaveChangesAsync();

            // ✅ Generar PDF
            string html = GenerarHtmlFactura(factura.IdFactura);
            string rutaRelativa = $"facturas/factura_{factura.IdFactura}.pdf";
            string rutaAbsoluta = Path.Combine(_env.WebRootPath, rutaRelativa);
            Directory.CreateDirectory(Path.GetDirectoryName(rutaAbsoluta)!);

            var pdf = PdfGenerator.GeneratePdf(html, PdfSharpCore.PageSize.A4);
            pdf.Save(rutaAbsoluta);

            factura.RutaPdf = rutaRelativa;
            await _context.SaveChangesAsync();

            return factura;
        }


        private string GenerarHtmlFactura(int idFactura)
        {
            var factura = _context.Facturas
                .Include(f => f.FacturaDetalles).ThenInclude(fd => fd.IdProductoNavigation)
                .Include(f => f.IdUsuarioNavigation)
                .First(f => f.IdFactura == idFactura);

            var sb = new StringBuilder();
            sb.AppendLine("<h2>Factura de Compra</h2>");
            sb.AppendLine($"<p>Factura N°: {factura.IdFactura}</p>");
            sb.AppendLine($"<p>Cliente: {factura.IdUsuarioNavigation.Nombre}</p>");
            sb.AppendLine($"<p>Correo: {factura.IdUsuarioNavigation.Correo}</p>");
            sb.AppendLine($"<p>Fecha: {factura.Fecha}</p>");
            sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0' width='100%'>");
            sb.AppendLine("<tr><th>Producto</th><th>Cantidad</th><th>Precio Unitario</th><th>Total</th></tr>");
            foreach (var detalle in factura.FacturaDetalles)
            {
                decimal subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                sb.AppendLine($"<tr><td>{detalle.IdProductoNavigation.Nombre}</td><td>{detalle.Cantidad}</td><td>${detalle.PrecioUnitario}</td><td>${subtotal}</td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine($"<h3>Total: ${factura.Total}</h3>");
            return sb.ToString();
        }
    }
}

