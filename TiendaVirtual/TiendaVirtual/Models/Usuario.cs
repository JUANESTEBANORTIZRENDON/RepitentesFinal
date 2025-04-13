using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Models;

[Table("usuario")]
[Index("Correo", Name = "usuario_correo_key", IsUnique = true)]
public partial class Usuario
{
    [Key]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("direccion")]
    [StringLength(150)]
    public string? Direccion { get; set; }

    [Column("telefono")]
    [StringLength(20)]
    public string? Telefono { get; set; }

    [Column("edad")]
    public int? Edad { get; set; }

    [Column("correo")]
    [StringLength(100)]
    public string Correo { get; set; } = null!;

    [Column("clave")]
    [StringLength(255)]
    public string Clave { get; set; } = null!;

    [Column("id_rol")]
    public int? IdRol { get; set; }

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<HistorialCompra> HistorialCompras { get; set; } = new List<HistorialCompra>();

    [ForeignKey("IdRol")]
    [InverseProperty("Usuarios")]
    public virtual Rol? IdRolNavigation { get; set; }
}
