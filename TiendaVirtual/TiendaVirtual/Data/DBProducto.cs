using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TiendaVirtual.Models;

namespace TiendaVirtual.Data
{
    public class DBProducto
    {
        private readonly ApplicationDbContext _context;
        
        public DBProducto(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtener todos los productos, incluyendo activos e inactivos si se especifica
        /// </summary>
        public async Task<List<Producto>> ObtenerProductosAsync(bool soloActivos = false)
        {
            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .AsQueryable();

            if (soloActivos)
            {
                query = query.Where(p => p.Activo == true || p.Activo == null);
            }

            return await query.OrderBy(p => p.IdProducto).ToListAsync();
        }

        /// <summary>
        /// Obtener un producto por su ID
        /// </summary>
        public async Task<Producto> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(p => p.IdProducto == id);
        }
        
        /// <summary>
        /// Verifica si hay suficiente stock de un producto para la cantidad solicitada
        /// </summary>
        public async Task<bool> VerificarStockDisponibleAsync(int idProducto, int cantidadSolicitada)
        {
            var producto = await _context.Productos.FindAsync(idProducto);
            return producto != null && producto.Stock >= cantidadSolicitada;
        }

        /// <summary>
        /// Insertar un nuevo producto
        /// </summary>
        public async Task<bool> InsertarProductoAsync(Producto producto)
        {
            try
            {
                // Establecer valores predeterminados para campos nulos
                producto.Activo = producto.Activo ?? true;
                
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Actualizar un producto existente
        /// </summary>
        public async Task<bool> ActualizarProductoAsync(Producto producto)
        {
            try
            {
                // Verificar que el producto existe
                var existente = await _context.Productos.FindAsync(producto.IdProducto);
                if (existente == null) return false;
                
                // Actualizar las propiedades
                existente.Nombre = producto.Nombre;
                existente.CodigoProducto = producto.CodigoProducto;
                existente.Marca = producto.Marca;
                existente.PrecioUnitario = producto.PrecioUnitario;
                existente.Stock = producto.Stock;
                existente.Imagen = producto.Imagen;
                existente.IdCategoria = producto.IdCategoria;
                existente.Activo = producto.Activo ?? true;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Actualizar el stock de un producto
        /// </summary>
        public async Task<bool> ActualizarStockAsync(int idProducto, int nuevoStock)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(idProducto);
                if (producto == null) return false;
                
                producto.Stock = nuevoStock;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Eliminar un producto (establecer como inactivo)
        /// </summary>
        public async Task<bool> EliminarProductoAsync(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null) return false;
                
                producto.Activo = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Cambiar el estado activo/inactivo de un producto
        /// </summary>
        public async Task<bool> CambiarEstadoProductoAsync(int id, bool activo)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null) return false;
                
                producto.Activo = activo;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtener todas las categorías disponibles para dropdowns
        /// </summary>
        public async Task<List<Categorium>> ObtenerCategoriasAsync()
        {
            return await _context.Categoria
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtener productos filtrados con paginación
        /// </summary>
        public async Task<(List<Producto> productos, int total)> ObtenerProductosFiltradosAsync(string busqueda, int pagina, int tamanoPagina, bool soloActivos = true)
        {
            // Construir la consulta base
            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .AsQueryable();
            
            // Aplicar filtros
            if (soloActivos)
            {
                query = query.Where(p => p.Activo == true || p.Activo == null);
            }
            
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p => 
                    p.Nombre.Contains(busqueda) || 
                    p.Marca.Contains(busqueda) || 
                    p.CodigoProducto.Contains(busqueda));
            }
            
            // Obtener el total para la paginación
            int total = await query.CountAsync();
            
            // Aplicar paginación
            var productos = await query
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();
            
            return (productos, total);
        }
        
        /// <summary>
        /// Obtener productos para catálogo con paginación y filtrado
        /// </summary>
        public async Task<(List<Producto> productos, int total)> ObtenerProductosCatalogoAsync(string busqueda, string orden, int pagina, int tamanoPagina)
        {
            // Construir consulta base
            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Where(p => p.Activo == true || p.Activo == null)
                .AsQueryable();

            // Aplicar filtro de búsqueda si se proporcionó
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    p.Marca.Contains(busqueda) ||
                    p.CodigoProducto.Contains(busqueda));
            }

            // Aplicar ordenamiento
            switch (orden)
            {
                case "precio_asc":
                    query = query.OrderBy(p => p.PrecioUnitario);
                    break;
                case "precio_desc":
                    query = query.OrderByDescending(p => p.PrecioUnitario);
                    break;
                case "stock_asc":
                    query = query.OrderBy(p => p.Stock);
                    break;
                case "stock_desc":
                    query = query.OrderByDescending(p => p.Stock);
                    break;
                default:
                    query = query.OrderBy(p => p.Nombre);
                    break;
            }

            // Obtener el total para paginación
            int total = await query.CountAsync();

            // Aplicar paginación
            var productos = await query
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            return (productos, total);
        }

        /// <summary>
        /// Verifica si ya existe un producto con el mismo código, nombre o marca
        /// </summary>
        public async Task<bool> ProductoExisteAsync(string codigo, string nombre, string marca, int? idExcluir = null)
        {
            var query = _context.Productos.AsQueryable();
            
            query = query.Where(p => 
                p.CodigoProducto == codigo || 
                p.Nombre == nombre || 
                p.Marca == marca);
                
            if (idExcluir.HasValue)
            {
                query = query.Where(p => p.IdProducto != idExcluir.Value);
            }
            
            return await query.AnyAsync();
        }

    }
}
