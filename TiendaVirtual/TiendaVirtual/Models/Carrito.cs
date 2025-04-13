using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Models;

[Table("carrito")]
public partial class Carrito
{
    [Key]
    [Column("id_carrito")]
    public int IdCarrito { get; set; }

    [Column("id_usuario")]
    public int? IdUsuario { get; set; }

    [Column("fecha_actualizacion", TypeName = "timestamp without time zone")]
    public DateTime? FechaActualizacion { get; set; }

    [InverseProperty("IdCarritoNavigation")]
    public virtual ICollection<CarritoProducto> CarritoProductos { get; set; } = new List<CarritoProducto>();

    [ForeignKey("IdUsuario")]
    [InverseProperty("Carritos")]
    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
