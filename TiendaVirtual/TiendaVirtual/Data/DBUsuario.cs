using TiendaVirtual.Models;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Data
{
    public class DBUsuario
    {
        private readonly TiendaDbContext _context;

        public DBUsuario(TiendaDbContext context)
        {
            _context = context;
        }

        // Verificar existencia del correo
        public bool ExisteCorreo(string correo)
        {
            return _context.Usuarios.Any(u => u.Correo == correo);
        }

        // Registrar nuevo cliente
        public async Task RegistrarClienteAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        // Buscar usuario por correo y clave
        public async Task<Usuario?> LoginAsync(string correo, string claveHash)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo && u.Clave == claveHash);
        }

        // Obtener nombre del rol por ID
        public async Task<string> ObtenerNombreRol(int? idRol)
        {
            if (idRol == null) return "SinRol";
            var rol = await _context.Rols.FindAsync(idRol);
            return rol?.Nombre ?? "SinRol";
        }

        // Buscar usuario por correo
        public async Task<Usuario?> BuscarPorCorreoAsync(string correo)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
        }

        // Cambiar contraseña
        public async Task CambiarClaveAsync(Usuario usuario, string nuevaClaveHash)
        {
            usuario.Clave = nuevaClaveHash;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}
