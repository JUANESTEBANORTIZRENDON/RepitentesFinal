using System;
using System.Collections.Generic;

namespace TiendaVirtual.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public int? Edad { get; set; }

    public string Correo { get; set; } = null!;

    public string Clave { get; set; } = null!;

    public int? IdRol { get; set; }

    public virtual ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual ICollection<HistorialCompra> HistorialCompras { get; set; } = new List<HistorialCompra>();

    public virtual Rol? IdRolNavigation { get; set; }
}
