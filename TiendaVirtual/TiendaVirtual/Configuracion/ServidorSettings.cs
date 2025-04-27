// Ruta: Models/ServidorSettings.cs

namespace TiendaVirtual.Models
{
    /// <summary>
    /// Clase para mapear la configuración del servidor desde appsettings.json.
    /// </summary>
    public class ServidorSettings
    {
        public string IpLocal { get; set; }
        public string Puerto { get; set; }
    }
}