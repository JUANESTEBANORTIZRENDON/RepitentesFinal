using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Models;

[Table("factura_detalle")]
public partial class FacturaDetalle
{
    [Key]
    [Column("id_detalle")]
    public int IdDetalle { get; set; }

    [Column("id_factura")]
    public int? IdFactura { get; set; }

    [Column("id_producto")]
    public int? IdProducto { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario")]
    [Precision(10, 2)]
    public decimal PrecioUnitario { get; set; }

    [ForeignKey("IdFactura")]
    [InverseProperty("FacturaDetalles")]
    public virtual Factura? IdFacturaNavigation { get; set; }

    [ForeignKey("IdProducto")]
    [InverseProperty("FacturaDetalles")]
    public virtual Producto? IdProductoNavigation { get; set; }
}
