using System;
using System.Collections.Generic;

namespace TiendaVirtual.Models;

public partial class HistorialCompra
{
    public int IdHistorial { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdFactura { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Factura? IdFacturaNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
