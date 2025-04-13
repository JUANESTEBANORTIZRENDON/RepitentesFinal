using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Models;

[Table("producto")]
[Index("CodigoProducto", Name = "producto_codigo_producto_key", IsUnique = true)]
public partial class Producto
{
    [Key]
    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("codigo_producto")]
    [StringLength(50)]
    public string CodigoProducto { get; set; } = null!;

    [Column("marca")]
    [StringLength(50)]
    public string? Marca { get; set; }

    [Column("precio_unitario")]
    [Precision(10, 2)]
    public decimal PrecioUnitario { get; set; }

    [Column("stock")]
    public int Stock { get; set; }

    [Column("imagen")]
    [StringLength(255)]
    public string? Imagen { get; set; }

    [InverseProperty("IdProductoNavigation")]
    public virtual ICollection<CarritoProducto> CarritoProductos { get; set; } = new List<CarritoProducto>();

    [InverseProperty("IdProductoNavigation")]
    public virtual ICollection<FacturaDetalle> FacturaDetalles { get; set; } = new List<FacturaDetalle>();
}
