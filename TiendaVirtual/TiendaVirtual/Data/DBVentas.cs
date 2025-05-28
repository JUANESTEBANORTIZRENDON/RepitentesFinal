using Microsoft.EntityFrameworkCore;
using TiendaVirtual.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaVirtual.Data
{
    // Clase auxiliar para representar las ventas mensuales
    public class VentaMensual
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
    }
    
    public class DBVentas
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DBVentas(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Obtiene la lista de facturas aplicando filtros opcionales
        /// </summary>
        public async Task<List<Factura>> ObtenerFacturasAsync(string busqueda, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Consulta base para obtener facturas con datos de usuario
            var query = _context.Facturas
                .Include(f => f.IdUsuarioNavigation)
                .AsQueryable();

            // Aplicar filtros si se proporcionaron
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                // Buscar por nombre de usuario, correo o ID de factura
                query = query.Where(f => 
                    f.IdUsuarioNavigation.Nombre.Contains(busqueda) ||
                    f.IdUsuarioNavigation.Correo.Contains(busqueda) ||
                    f.IdFactura.ToString() == busqueda);
            }

            // Filtrar por fecha inicial si se proporcionó
            if (fechaDesde.HasValue)
            {
                // Ajustar para incluir todo el día de inicio
                var fechaDesdeAjustada = fechaDesde.Value.Date;
                query = query.Where(f => f.Fecha >= fechaDesdeAjustada);
            }

            // Filtrar por fecha final si se proporcionó
            if (fechaHasta.HasValue)
            {
                // Ajustar para incluir todo el día final
                var fechaHastaAjustada = fechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(f => f.Fecha <= fechaHastaAjustada);
            }

            // Ordenar por fecha descendente y ejecutar la consulta
            return await query
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una factura con todos sus detalles
        /// </summary>
        public async Task<Factura> ObtenerFacturaPorIdAsync(int id)
        {
            return await _context.Facturas
                .Include(f => f.IdUsuarioNavigation)
                .Include(f => f.FacturaDetalles)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(f => f.IdFactura == id);
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        public async Task<Usuario> ObtenerUsuarioPorIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        /// <summary>
        /// Obtiene el historial de facturas de un usuario
        /// </summary>
        public async Task<List<Factura>> ObtenerHistorialUsuarioAsync(int idUsuario)
        {
            return await _context.Facturas
                .Where(f => f.IdUsuario == idUsuario)
                .Include(f => f.FacturaDetalles)
                    .ThenInclude(d => d.IdProductoNavigation)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una factura con su ruta de PDF
        /// </summary>
        public async Task<Factura> ObtenerFacturaParaDescargarAsync(int id)
        {
            return await _context.Facturas.FindAsync(id);
        }

        /// <summary>
        /// Obtiene los bytes del archivo PDF de una factura
        /// </summary>
        public async Task<byte[]> ObtenerBytesFacturaPdfAsync(string rutaPdf)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", rutaPdf);
            return await File.ReadAllBytesAsync(path);
        }

        /// <summary>
        /// Obtiene las ventas agrupadas por mes para un rango de fechas
        /// </summary>
        public async Task<List<VentaMensual>> ObtenerVentasPorMesAsync(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Creamos una lista para almacenar los resultados
            List<VentaMensual> ventasPorMes = new List<VentaMensual>();
            
            try
            {
                // Traemos todos los datos relevantes desde la base de datos (solo lo que necesitamos)
                var facturas = await _context.Facturas
                    .Where(f => 
                        (!fechaDesde.HasValue || f.Fecha >= fechaDesde.Value.Date) &&
                        (!fechaHasta.HasValue || f.Fecha <= fechaHasta.Value.Date.AddDays(1).AddSeconds(-1))
                    )
                    .Select(f => new
                    {
                        f.Fecha,
                        f.Total
                    })
                    .ToListAsync();
                
                if (facturas.Any())
                {
                    // Procesamos los datos en memoria
                    // Usamos un diccionario para acumular las ventas por año y mes
                    var ventasAcumuladas = new Dictionary<(int anio, int mes), (decimal total, int cantidad)>();
                    
                    foreach (var factura in facturas)
                    {
                        // Ignoramos facturas con fecha nula
                        if (factura.Fecha.HasValue && factura.Total.HasValue)
                        {
                            int anio = factura.Fecha.Value.Year;
                            int mes = factura.Fecha.Value.Month;
                            decimal total = factura.Total.Value;
                            
                            var clave = (anio, mes);
                            if (ventasAcumuladas.ContainsKey(clave))
                            {
                                var actual = ventasAcumuladas[clave];
                                ventasAcumuladas[clave] = (actual.total + total, actual.cantidad + 1);
                            }
                            else
                            {
                                ventasAcumuladas[clave] = (total, 1);
                            }
                        }
                    }
                    
                    // Convertimos el diccionario a la lista de VentaMensual
                    ventasPorMes = ventasAcumuladas
                        .Select(kv => new VentaMensual
                        {
                            Anio = kv.Key.anio,
                            Mes = kv.Key.mes,
                            NombreMes = ObtenerNombreMes(kv.Key.mes),
                            TotalVentas = kv.Value.total,
                            CantidadVentas = kv.Value.cantidad
                        })
                        .OrderByDescending(v => v.Anio)
                        .ThenByDescending(v => v.Mes)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                // Registrar la excepción (aquí podrías usar un sistema de logging)
                Console.WriteLine($"Error al obtener ventas por mes: {ex.Message}");
                // Retornar lista vacía en caso de error
            }
            
            return ventasPorMes;
        }
        
        /// <summary>
        /// Obtiene el nombre del mes a partir del número
        /// </summary>
        private string ObtenerNombreMes(int mes)
        {
            return mes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => "Desconocido"
            };
        }
    }
}
