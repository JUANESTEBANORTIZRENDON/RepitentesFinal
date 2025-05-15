using Microsoft.AspNetCore.Mvc;
using TiendaVirtual.Models;
using TiendaVirtual.Data;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Controllers;

public class CategoriaController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var categorias = _context.Categoria.ToList(); // List<Categorium>
        ViewBag.Categorias = categorias; // opcional si usas ViewBag
        return View(categorias); // ✅ MUY IMPORTANTE: este debe ser el modelo esperado por Index.cshtml
    }




    public IActionResult Crear()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Categorium categoria)
    {
        if (ModelState.IsValid)
        {
            _context.Categoria.Add(categoria);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(categoria);
    }

    public async Task<IActionResult> Editar(int id)
    {
        var categoria = await _context.Categoria.FindAsync(id);
        if (categoria == null) return NotFound();
        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Categorium categoria)
    {
        if (ModelState.IsValid)
        {
            _context.Categoria.Update(categoria);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(categoria);
    }

    public async Task<IActionResult> Eliminar(int id)
    {
        var categoria = await _context.Categoria.FindAsync(id);
        if (categoria == null) return NotFound();

        // Validar si hay productos asociados
        bool tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id);

        if (tieneProductos)
        {
            TempData["error"] = "No se puede eliminar la categoría porque tiene productos asociados.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categoria.Remove(categoria);
        await _context.SaveChangesAsync();

        TempData["mensaje"] = "Categoría eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }


    public IActionResult ProductosPorCategoria(int id)
    {
        var productos = _context.Productos
            .Where(p => p.IdCategoria == id)
            .ToList();

        ViewBag.CategoriaSeleccionada = _context.Categoria
            .FirstOrDefault(c => c.IdCategoria == id)?.Nombre;

        return PartialView("_ProductosPorCategoria", productos);
    }
}

