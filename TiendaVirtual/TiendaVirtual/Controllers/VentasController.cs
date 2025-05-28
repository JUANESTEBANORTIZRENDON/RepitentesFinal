using Microsoft.AspNetCore.Mvc;
using TiendaVirtual.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TiendaVirtual.Data;
using System.Collections.Generic;

namespace TiendaVirtual.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class VentasController : Controller
    {
        private readonly DBVentas _dbVentas;

        public VentasController(DBVentas dbVentas)
        {
            _dbVentas = dbVentas;
        }

        // GET: Ventas
        public async Task<IActionResult> Index(string busqueda, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Utilizar la clase auxiliar para obtener las facturas con filtros
            var facturas = await _dbVentas.ObtenerFacturasAsync(busqueda, fechaDesde, fechaHasta);

            // Guardar los parámetros de búsqueda para mantenerlos en la vista
            ViewBag.Busqueda = busqueda;
            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");

            return View(facturas);
        }

        // GET: Ventas/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener la factura con todos sus detalles y datos relacionados
            var factura = await _dbVentas.ObtenerFacturaPorIdAsync(id.Value);

            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // GET: Ventas/HistorialUsuario/5
        public async Task<IActionResult> HistorialUsuario(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener el usuario
            var usuario = await _dbVentas.ObtenerUsuarioPorIdAsync(id.Value);
            if (usuario == null)
            {
                return NotFound();
            }

            // Obtener todas las facturas del usuario
            var facturas = await _dbVentas.ObtenerHistorialUsuarioAsync(id.Value);

            ViewBag.Usuario = usuario;
            return View(facturas);
        }

        // GET: Ventas/DescargarFactura/5
        public async Task<IActionResult> DescargarFactura(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _dbVentas.ObtenerFacturaParaDescargarAsync(id.Value);
            if (factura == null || string.IsNullOrEmpty(factura.RutaPdf))
            {
                return NotFound("Factura no encontrada o sin PDF.");
            }

            try
            {
                var pdfBytes = await _dbVentas.ObtenerBytesFacturaPdfAsync(factura.RutaPdf);
                return File(pdfBytes, "application/pdf", $"factura_{factura.IdFactura}.pdf");
            }
            catch (System.IO.FileNotFoundException)
            {
                return NotFound("Archivo PDF no encontrado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al descargar factura: {ex.Message}");
            }
        }
        
        // GET: Ventas/VentasMensuales
        public async Task<IActionResult> VentasMensuales(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Si no se especifica un rango de fechas, usamos el año actual
            if (!fechaDesde.HasValue)
            {
                fechaDesde = new DateTime(DateTime.Now.Year, 1, 1);
            }
            
            if (!fechaHasta.HasValue)
            {
                fechaHasta = DateTime.Now;
            }
            
            // Obtener las ventas mensuales
            var ventasMensuales = await _dbVentas.ObtenerVentasPorMesAsync(fechaDesde, fechaHasta);
            
            // Calcular totales generales
            decimal totalGeneral = ventasMensuales.Sum(v => v.TotalVentas);
            int cantidadGeneral = ventasMensuales.Sum(v => v.CantidadVentas);
            
            ViewBag.TotalGeneral = totalGeneral;
            ViewBag.CantidadGeneral = cantidadGeneral;
            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");
            
            return View(ventasMensuales);
        }
    }
}
