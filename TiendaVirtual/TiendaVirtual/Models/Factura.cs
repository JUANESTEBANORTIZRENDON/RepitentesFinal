using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Models;

[Table("factura")]
public partial class Factura
{
    [Key]
    [Column("id_factura")]
    public int IdFactura { get; set; }

    [Column("id_usuario")]
    public int? IdUsuario { get; set; }

    [Column("fecha", TypeName = "timestamp without time zone")]
    public DateTime? Fecha { get; set; }

    [Column("total")]
    [Precision(12, 2)]
    public decimal? Total { get; set; }

    [Column("ruta_pdf")]
    [StringLength(255)]
    public string? RutaPdf { get; set; }

    [InverseProperty("IdFacturaNavigation")]
    public virtual ICollection<FacturaDetalle> FacturaDetalles { get; set; } = new List<FacturaDetalle>();

    [InverseProperty("IdFacturaNavigation")]
    public virtual ICollection<HistorialCompra> HistorialCompras { get; set; } = new List<HistorialCompra>();

    [ForeignKey("IdUsuario")]
    [InverseProperty("Facturas")]
    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
