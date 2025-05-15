using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TiendaVirtual.Models;

public partial class Categorium
{
    [Key]
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
