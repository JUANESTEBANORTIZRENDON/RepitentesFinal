using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Models;

[Table("historial_compra")]
public partial class HistorialCompra
{
    [Key]
    [Column("id_historial")]
    public int IdHistorial { get; set; }

    [Column("id_usuario")]
    public int? IdUsuario { get; set; }

    [Column("id_factura")]
    public int? IdFactura { get; set; }

    [Column("fecha", TypeName = "timestamp without time zone")]
    public DateTime? Fecha { get; set; }

    [ForeignKey("IdFactura")]
    [InverseProperty("HistorialCompras")]
    public virtual Factura? IdFacturaNavigation { get; set; }

    [ForeignKey("IdUsuario")]
    [InverseProperty("HistorialCompras")]
    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
