using Npgsql;
using TiendaVirtual.Models;

namespace TiendaVirtual.Data;

public static class DBCarrito
{
    private static readonly string connectionString = "TU_CADENA_DE_CONEXION";

    // Obtener el carrito por usuario, lo crea si no existe
    public static int ObtenerOCrearCarrito(int idUsuario)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        string select = "SELECT id_carrito FROM carrito WHERE id_usuario = @id_usuario";
        using var selectCmd = new NpgsqlCommand(select, conn);
        selectCmd.Parameters.AddWithValue("id_usuario", idUsuario);

        var result = selectCmd.ExecuteScalar();
        if (result != null)
            return (int)result;

        string insert = "INSERT INTO carrito (id_usuario) VALUES (@id_usuario) RETURNING id_carrito";
        using var insertCmd = new NpgsqlCommand(insert, conn);
        insertCmd.Parameters.AddWithValue("id_usuario", idUsuario);
        return (int)insertCmd.ExecuteScalar();
    }

    // Agregar producto o actualizar cantidad
    public static void AgregarProducto(int idUsuario, int idProducto, int cantidad)
    {
        var idCarrito = ObtenerOCrearCarrito(idUsuario);
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        string existe = "SELECT cantidad FROM carrito_producto WHERE id_carrito = @carrito AND id_producto = @producto";
        using var checkCmd = new NpgsqlCommand(existe, conn);
        checkCmd.Parameters.AddWithValue("carrito", idCarrito);
        checkCmd.Parameters.AddWithValue("producto", idProducto);
        var actual = checkCmd.ExecuteScalar();

        if (actual != null)
        {
            string update = "UPDATE carrito_producto SET cantidad = cantidad + @cantidad WHERE id_carrito = @carrito AND id_producto = @producto";
            using var updateCmd = new NpgsqlCommand(update, conn);
            updateCmd.Parameters.AddWithValue("carrito", idCarrito);
            updateCmd.Parameters.AddWithValue("producto", idProducto);
            updateCmd.Parameters.AddWithValue("cantidad", cantidad);
            updateCmd.ExecuteNonQuery();
        }
        else
        {
            string precioQuery = "SELECT precio_unitario FROM producto WHERE id_producto = @id_producto";
            using var precioCmd = new NpgsqlCommand(precioQuery, conn);
            precioCmd.Parameters.AddWithValue("id_producto", idProducto);
            decimal precio = (decimal)precioCmd.ExecuteScalar();

            string insert = @"INSERT INTO carrito_producto (id_carrito, id_producto, cantidad, precio_unitario)
                              VALUES (@carrito, @producto, @cantidad, @precio)";
            using var insertCmd = new NpgsqlCommand(insert, conn);
            insertCmd.Parameters.AddWithValue("carrito", idCarrito);
            insertCmd.Parameters.AddWithValue("producto", idProducto);
            insertCmd.Parameters.AddWithValue("cantidad", cantidad);
            insertCmd.Parameters.AddWithValue("precio", precio);
            insertCmd.ExecuteNonQuery();
        }
    }

    public static List<CarritoProducto> ObtenerProductosCarrito(int idUsuario)
    {
        var productos = new List<CarritoProducto>();
        var idCarrito = ObtenerOCrearCarrito(idUsuario);

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        string query = @"SELECT cp.*, p.nombre, p.imagen FROM carrito_producto cp
                         INNER JOIN producto p ON p.id_producto = cp.id_producto
                         WHERE cp.id_carrito = @carrito";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("carrito", idCarrito);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            productos.Add(new CarritoProducto
            {
                IdCarrito = (int)reader["id_carrito"],
                IdProducto = (int)reader["id_producto"],
                Cantidad = (int)reader["cantidad"],
                PrecioUnitario = (decimal)reader["precio_unitario"],
                IdProductoNavigation = new Producto
                {
                    Nombre = reader["nombre"].ToString(),
                    Imagen = reader["imagen"].ToString()
                }
            });
        }

        return productos;
    }

    // Eliminar producto del carrito
    public static void EliminarProducto(int idUsuario, int idProducto)
    {
        int idCarrito = ObtenerOCrearCarrito(idUsuario);
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        string query = "DELETE FROM carrito_producto WHERE id_carrito = @carrito AND id_producto = @producto";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("carrito", idCarrito);
        cmd.Parameters.AddWithValue("producto", idProducto);
        cmd.ExecuteNonQuery();
    }

    // Cambiar cantidad
    public static void CambiarCantidad(int idUsuario, int idProducto, int nuevaCantidad)
    {
        int idCarrito = ObtenerOCrearCarrito(idUsuario);
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        string query = "UPDATE carrito_producto SET cantidad = @cantidad WHERE id_carrito = @carrito AND id_producto = @producto";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("carrito", idCarrito);
        cmd.Parameters.AddWithValue("producto", idProducto);
        cmd.Parameters.AddWithValue("cantidad", nuevaCantidad);
        cmd.ExecuteNonQuery();
    }
}

