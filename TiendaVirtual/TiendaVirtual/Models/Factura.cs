using System;
using System.Collections.Generic;

namespace TiendaVirtual.Models;

public partial class Factura
{
    public int IdFactura { get; set; }

    public int? IdUsuario { get; set; }

    public DateTime? Fecha { get; set; }

    public decimal? Total { get; set; }

    public string? RutaPdf { get; set; }

    public virtual ICollection<FacturaDetalle> FacturaDetalles { get; set; } = new List<FacturaDetalle>();

    public virtual ICollection<HistorialCompra> HistorialCompras { get; set; } = new List<HistorialCompra>();

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
