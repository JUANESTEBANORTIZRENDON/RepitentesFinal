using TiendaVirtual.Models;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Data;

public class DBUsuario
{
    private readonly ApplicationDbContext _context;

    public DBUsuario(ApplicationDbContext context) => _context = context;

    // Registrar usuario
    public async Task RegistrarUsuarioAsync(Usuario usuario)
    {
        usuario.Clave = BCrypt.Net.BCrypt.HashPassword(usuario.Clave);
        usuario.IdRol = 2; // Cliente
        usuario.Confirmado = false;
        usuario.TokenConfirmacion = Guid.NewGuid().ToString();
        _context.Add(usuario);
        await _context.SaveChangesAsync();
    }

    // Confirmar cuenta
    public async Task<bool> ConfirmarCuentaAsync(string token)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.TokenConfirmacion == token);
        if (usuario == null) return false;
        usuario.Confirmado = true;
        usuario.TokenConfirmacion = null;
        await _context.SaveChangesAsync();
        return true;
    }

    // Método para validar el login 
    public async Task<Usuario> LoginAsync(string correo, string clave)
    {
        // Aquí verifica la contraseña como la tengas almacenada (hasheada o directa)
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == correo && u.Confirmado);

        if (usuario != null && BCrypt.Net.BCrypt.Verify(clave, usuario.Clave))
        {
            return usuario;
        }

        return null;
    }

    // Generar token recuperación
    public async Task<bool> GenerarTokenRecuperacionAsync(string correo)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo && u.Confirmado);

        if (usuario == null)
            return false; // No existe el usuario o no está confirmado

        usuario.TokenRecuperacion = Guid.NewGuid().ToString();
        usuario.ExpiracionTokenRecuperacion = DateTime.Now.AddHours(1);
      

        await _context.SaveChangesAsync();
        return true; 
    }



    // Restablecer contraseña

    public async Task<bool> RestablecerPasswordAsync(string token, string nuevaClave)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(nuevaClave))
            return false; // Validamos entradas

        var fechaActual = DateTime.Now;

        // Buscamos el usuario con el token y que el token no haya expirado
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.TokenRecuperacion == token && u.ExpiracionTokenRecuperacion > fechaActual);

        if (usuario == null)
            return false; // Si no encuentra el usuario o el token expiró

        // Cambiamos la contraseña
        usuario.Clave = BCrypt.Net.BCrypt.HashPassword(nuevaClave);

        // Limpiamos el token y la expiración para que no se reutilice
        usuario.TokenRecuperacion = null;
        usuario.ExpiracionTokenRecuperacion = null;

        // Marcamos el usuario como modificado
        _context.Usuarios.Update(usuario); // Aquí estaba el problema: no se marcaba como modificado

        // Guardamos los cambios
        var cambios = await _context.SaveChangesAsync();
        return cambios > 0; // Retornamos true si se guardó al menos un cambio
    }



    //Metodo para obtener rol de usuario 
    public async Task<string> ObtenerNombreRolAsync(int idRol)
    {
        var rol = await _context.Rols.FirstOrDefaultAsync(r => r.IdRol == idRol);
        return rol?.Nombre ?? "SinRol";
    }

    
    public async Task<Usuario> ObtenerUsuarioPorCorreoAsync(string correo)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo && u.Confirmado);
    }


    // Verifica si un correo ya está registrado (sin importar si está confirmado o no)
    public async Task<bool> ExisteCorreoAsync(string correo)
    {
        return await _context.Usuarios.AnyAsync(u => u.Correo == correo);
    }


    // Método para verificar si la nueva clave es igual a la actual SIN usar el token
    public async Task<bool> ClaveEsIgualAsync(int idUsuario, string nuevaClave)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            throw new InvalidOperationException("Usuario no encontrado.");

        // Comparamos la nueva contraseña en texto plano contra la contraseña hasheada guardada
        return BCrypt.Net.BCrypt.Verify(nuevaClave, usuario.Clave);
    }




    // Método para obtener el usuario por token y validar expiración del token
    public async Task<Usuario> ObtenerUsuarioPorTokenAsync(string token)
    {
        var fechaActual = DateTime.Now;
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.TokenRecuperacion == token && u.ExpiracionTokenRecuperacion > fechaActual);
    }



}

