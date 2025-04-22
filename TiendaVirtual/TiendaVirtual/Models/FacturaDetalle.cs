using System;
using System.Collections.Generic;

namespace TiendaVirtual.Models;

public partial class FacturaDetalle
{
    public int IdDetalle { get; set; }

    public int? IdFactura { get; set; }

    public int? IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public virtual Factura? IdFacturaNavigation { get; set; }

    public virtual Producto? IdProductoNavigation { get; set; }
}
