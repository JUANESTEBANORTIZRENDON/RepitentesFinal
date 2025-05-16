using TiendaVirtual.Models;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Data
{
    public class DBHistorial
    {
        private readonly ApplicationDbContext _context;

        public DBHistorial(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<HistorialCompra>> ObtenerHistorialPorUsuarioAsync(int idUsuario)
        {
            return await _context.HistorialCompras
                .Include(h => h.IdFacturaNavigation)
                .Where(h => h.IdUsuario == idUsuario)
                .OrderByDescending(h => h.Fecha)
                .ToListAsync();
        }
    }
}
