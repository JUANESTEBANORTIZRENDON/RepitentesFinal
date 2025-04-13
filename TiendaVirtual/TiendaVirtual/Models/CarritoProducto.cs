using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Models;

[PrimaryKey("IdCarrito", "IdProducto")]
[Table("carrito_producto")]
public partial class CarritoProducto
{
    [Key]
    [Column("id_carrito")]
    public int IdCarrito { get; set; }

    [Key]
    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario")]
    [Precision(10, 2)]
    public decimal PrecioUnitario { get; set; }

    [ForeignKey("IdCarrito")]
    [InverseProperty("CarritoProductos")]
    public virtual Carrito IdCarritoNavigation { get; set; } = null!;

    [ForeignKey("IdProducto")]
    [InverseProperty("CarritoProductos")]
    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
