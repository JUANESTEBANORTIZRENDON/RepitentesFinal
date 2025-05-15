using System;
using System.Collections.Generic;

namespace TiendaVirtual.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public string CodigoProducto { get; set; } = null!;

    public string? Marca { get; set; }

    public decimal PrecioUnitario { get; set; }

    public int Stock { get; set; }

    public string? Imagen { get; set; }

    public int? IdCategoria { get; set; }

    public virtual ICollection<CarritoProducto> CarritoProductos { get; set; } = new List<CarritoProducto>();

    public virtual ICollection<FacturaDetalle> FacturaDetalles { get; set; } = new List<FacturaDetalle>();

    public virtual Categorium? IdCategoriaNavigation { get; set; }
}
