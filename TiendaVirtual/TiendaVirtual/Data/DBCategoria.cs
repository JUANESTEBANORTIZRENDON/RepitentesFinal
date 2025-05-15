using TiendaVirtual.Models;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Data;

public class DBCategoria
{
    private readonly ApplicationDbContext _context;

    public DBCategoria(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Categorium>> ObtenerTodasAsync()
    {
        return await _context.Categoria.ToListAsync();
    }

    public async Task<bool> CrearAsync(Categorium categoria)
    {
        _context.Categoria.Add(categoria);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ActualizarAsync(Categorium categoria)
    {
        _context.Categoria.Update(categoria);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var categoria = await _context.Categoria.FindAsync(id);
        if (categoria == null) return false;

        _context.Categoria.Remove(categoria);
        return await _context.SaveChangesAsync() > 0;
    }

    public List<Categorium> ObtenerCategorias()
    {
        return _context.Categoria.ToList();
    }



}


