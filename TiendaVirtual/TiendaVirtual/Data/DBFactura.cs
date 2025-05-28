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
        // Diccionario en memoria para almacenar PDFs temporalmente (solo para la sesión actual)
        private static Dictionary<int, byte[]> _pdfCache = new Dictionary<int, byte[]>();

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

            try
            {
                // Intentar guardar en el sistema de archivos (para desarrollo local)
                string rutaRelativa = $"facturas/factura_{factura.IdFactura}.pdf";

                // Generar el PDF en memoria primero
                var pdf = PdfGenerator.GeneratePdf(html, PdfSharpCore.PageSize.A4);

                // Guardar en caché de memoria
                using (MemoryStream ms = new MemoryStream())
                {
                    pdf.Save(ms);
                    _pdfCache[factura.IdFactura] = ms.ToArray();
                }

                try
                {
                    // Intentar guardar en disco (funcionará en desarrollo, puede fallar en producción)
                    string rutaAbsoluta = Path.Combine(_env.WebRootPath, rutaRelativa);
                    Directory.CreateDirectory(Path.GetDirectoryName(rutaAbsoluta)!);
                    pdf.Save(rutaAbsoluta);
                    factura.RutaPdf = rutaRelativa;
                }
                catch (Exception ex)
                {
                    // Si falla al guardar en disco, usar un identificador especial 
                    // para indicar que está en memoria
                    factura.RutaPdf = $"memory:{factura.IdFactura}";
                    Console.WriteLine($"Error al guardar PDF en disco: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // Si hay algún error general con la generación del PDF
                Console.WriteLine($"Error al generar PDF: {ex.Message}");
                factura.RutaPdf = null;
            }

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


        /// <summary>
        /// Obtiene un PDF de factura como array de bytes, intentando primero obtenerlo de memoria
        /// y luego del sistema de archivos si está disponible
        /// </summary>
        public async Task<byte[]> ObtenerPdfFacturaAsync(int idFactura)
        {
            // Buscar primero en la caché de memoria
            if (_pdfCache.ContainsKey(idFactura))
            {
                return _pdfCache[idFactura];
            }

            // Si no está en memoria, buscar en la base de datos
            var factura = await _context.Facturas.FindAsync(idFactura);
            if (factura == null || string.IsNullOrEmpty(factura.RutaPdf))
            {
                throw new FileNotFoundException("Factura no encontrada o sin PDF asociado");
            }

            // Si el PDF está en la memoria (identificado por el prefijo)
            if (factura.RutaPdf.StartsWith("memory:"))
            {
                // Regenerar el PDF si no está en caché
                string html = GenerarHtmlFactura(factura.IdFactura);
                var pdf = PdfGenerator.GeneratePdf(html, PdfSharpCore.PageSize.A4);

                using (MemoryStream ms = new MemoryStream())
                {
                    pdf.Save(ms);
                    byte[] pdfBytes = ms.ToArray();
                    // Almacenar en caché para futuros usos
                    _pdfCache[idFactura] = pdfBytes;
                    return pdfBytes;
                }
            }

            // Si llegamos aquí, el PDF debería estar en el sistema de archivos
            try
            {
                string rutaAbsoluta = Path.Combine(_env.WebRootPath, factura.RutaPdf);
                if (System.IO.File.Exists(rutaAbsoluta))
                {
                    byte[] pdfBytes = await System.IO.File.ReadAllBytesAsync(rutaAbsoluta);
                    // Almacenar en caché para futuros usos
                    _pdfCache[idFactura] = pdfBytes;
                    return pdfBytes;
                }
                else
                {
                    // Si el archivo no existe, regenerar el PDF
                    string html = GenerarHtmlFactura(factura.IdFactura);
                    var pdf = PdfGenerator.GeneratePdf(html, PdfSharpCore.PageSize.A4);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        pdf.Save(ms);
                        byte[] pdfBytes = ms.ToArray();
                        _pdfCache[idFactura] = pdfBytes;
                        return pdfBytes;
                    }
                }
            }
            catch
            {
                // Si falla al leer del disco, regenerar el PDF
                string html = GenerarHtmlFactura(factura.IdFactura);
                var pdf = PdfGenerator.GeneratePdf(html, PdfSharpCore.PageSize.A4);

                using (MemoryStream ms = new MemoryStream())
                {
                    pdf.Save(ms);
                    byte[] pdfBytes = ms.ToArray();
                    _pdfCache[idFactura] = pdfBytes;
                    return pdfBytes;
                }
            }
        }
    }
}


    
        


