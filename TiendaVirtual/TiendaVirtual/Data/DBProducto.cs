using Npgsql;
using System;
using System.Collections.Generic;
using TiendaVirtual.Models;

namespace TiendaVirtual.Data
{
    public static class DBProducto
    {
        // Cadena de conexión a Neon.tech
        private static readonly string connectionString = "Host=ep-blue-hat-a5e7rnyz-pooler.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_klWR3jf6EiHv;SSL Mode=Require;Trust Server Certificate=true";

        // Obtener todos los productos, incluyendo activos e inactivos
        public static List<Producto> ObtenerProductos(bool soloActivos = false)
        {
            var lista = new List<Producto>();

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM producto" + (soloActivos ? " WHERE activo = true OR activo IS NULL" : "") + " ORDER BY id_producto";
            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Producto
                {
                    IdProducto = (int)reader["id_producto"],
                    Nombre = reader["nombre"].ToString(),
                    CodigoProducto = reader["codigo_producto"].ToString(),
                    Marca = reader["marca"].ToString(),
                    PrecioUnitario = (decimal)reader["precio_unitario"],
                    Stock = (int)reader["stock"],
                    Imagen = reader["imagen"].ToString(),
                    IdCategoria = reader["id_categoria"] != DBNull.Value ? (int?)reader["id_categoria"] : null,
                    Activo = reader["activo"] != DBNull.Value ? (bool?)reader["activo"] : true
                });
            }

            return lista;
        }

        // Obtener un producto por su ID
        public static Producto ObtenerPorId(int id)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM producto WHERE id_producto = @id";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Producto
                {
                    IdProducto = (int)reader["id_producto"],
                    Nombre = reader["nombre"].ToString(),
                    CodigoProducto = reader["codigo_producto"].ToString(),
                    Marca = reader["marca"].ToString(),
                    PrecioUnitario = (decimal)reader["precio_unitario"],
                    Stock = (int)reader["stock"],
                    Imagen = reader["imagen"].ToString(),
                    IdCategoria = reader["id_categoria"] != DBNull.Value ? (int?)reader["id_categoria"] : null,
                    Activo = reader["activo"] != DBNull.Value ? (bool?)reader["activo"] : true
                };
            }

            return null;
        }

        // Insertar un nuevo producto
        public static bool InsertarProducto(Producto producto)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string query = @"INSERT INTO producto 
                (nombre, codigo_producto, marca, precio_unitario, stock, imagen, id_categoria, activo)
                VALUES (@nombre, @codigo, @marca, @precio, @stock, @imagen, @categoria, @activo)";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("nombre", producto.Nombre);
            cmd.Parameters.AddWithValue("codigo", producto.CodigoProducto);
            cmd.Parameters.AddWithValue("marca", (object)producto.Marca ?? DBNull.Value);
            cmd.Parameters.AddWithValue("precio", producto.PrecioUnitario);
            cmd.Parameters.AddWithValue("stock", producto.Stock);
            cmd.Parameters.AddWithValue("imagen", (object)producto.Imagen ?? DBNull.Value);
            cmd.Parameters.AddWithValue("categoria", (object)producto.IdCategoria ?? DBNull.Value);
            cmd.Parameters.AddWithValue("activo", producto.Activo ?? true);

            return cmd.ExecuteNonQuery() > 0;
        }

        // Actualizar un producto existente
        public static bool ActualizarProducto(Producto producto)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string query = @"UPDATE producto SET 
                nombre = @nombre, 
                codigo_producto = @codigo, 
                marca = @marca, 
                precio_unitario = @precio, 
                stock = @stock, 
                imagen = @imagen,
                id_categoria = @categoria,
                activo = @activo
                WHERE id_producto = @id";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("nombre", producto.Nombre);
            cmd.Parameters.AddWithValue("codigo", producto.CodigoProducto);
            cmd.Parameters.AddWithValue("marca", (object)producto.Marca ?? DBNull.Value);
            cmd.Parameters.AddWithValue("precio", producto.PrecioUnitario);
            cmd.Parameters.AddWithValue("stock", producto.Stock);
            cmd.Parameters.AddWithValue("imagen", (object)producto.Imagen ?? DBNull.Value);
            cmd.Parameters.AddWithValue("categoria", (object)producto.IdCategoria ?? DBNull.Value);
            cmd.Parameters.AddWithValue("activo", producto.Activo ?? true);
            cmd.Parameters.AddWithValue("id", producto.IdProducto);

            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool EliminarProducto(int id)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string query = "UPDATE producto SET activo = false WHERE id_producto = @id";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("id", id);

            return cmd.ExecuteNonQuery() > 0;
        }
        
        // Cambiar el estado activo/inactivo de un producto
        public static bool CambiarEstadoProducto(int id, bool activo)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string query = "UPDATE producto SET activo = @activo WHERE id_producto = @id";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("activo", activo);
            cmd.Parameters.AddWithValue("id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        // Obtener todas las categorías disponibles para dropdowns
        public static List<Categorium> ObtenerCategorias()
        {
            var lista = new List<Categorium>();

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM categoria ORDER BY nombre";
            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Categorium
                {
                    IdCategoria = (int)reader["id_categoria"],
                    Nombre = reader["nombre"].ToString()
                });
            }

            return lista;
        }

        public static List<Producto> ObtenerProductosFiltrados(string busqueda, int pagina, int tamanoPagina, out int total, bool soloActivos = true)
        {
            List<Producto> lista = new List<Producto>();
            total = 0;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Conteo total para paginación
                string activoCondition = soloActivos ? " AND (activo = true OR activo IS NULL)" : "";
                string countQuery = @"SELECT COUNT(*) FROM producto 
                              WHERE (@busqueda IS NULL OR 
                                     nombre ILIKE '%' || @busqueda || '%' OR 
                                     marca ILIKE '%' || @busqueda || '%' OR 
                                     codigo_producto ILIKE '%' || @busqueda || '%')"
                                     + activoCondition;

                using (var countCmd = new NpgsqlCommand(countQuery, conn))
                {
                    countCmd.Parameters.AddWithValue("busqueda", (object)busqueda ?? DBNull.Value);
                    total = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                // Consulta paginada
                string query = @"SELECT * FROM producto 
                         WHERE (@busqueda IS NULL OR 
                                nombre ILIKE '%' || @busqueda || '%' OR 
                                marca ILIKE '%' || @busqueda || '%' OR 
                                codigo_producto ILIKE '%' || @busqueda || '%')"
                                + activoCondition +
                         @" ORDER BY id_producto
                         OFFSET @offset LIMIT @limit";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("busqueda", (object)busqueda ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("offset", (pagina - 1) * tamanoPagina);
                    cmd.Parameters.AddWithValue("limit", tamanoPagina);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Producto
                            {
                                IdProducto = (int)reader["id_producto"],
                                Nombre = reader["nombre"].ToString(),
                                CodigoProducto = reader["codigo_producto"].ToString(),
                                Marca = reader["marca"].ToString(),
                                PrecioUnitario = (decimal)reader["precio_unitario"],
                                Stock = (int)reader["stock"],
                                Imagen = reader["imagen"].ToString(),
                                IdCategoria = reader["id_categoria"] != DBNull.Value ? (int?)reader["id_categoria"] : null,
                                Activo = reader["activo"] != DBNull.Value ? (bool?)reader["activo"] : true
                            });
                        }
                    }
                }
            }

            return lista;
        }

        // Verifica si ya existe un producto con el mismo código, nombre o marca
        public static bool ProductoExiste(string codigo, string nombre)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string query = @"SELECT COUNT(*) FROM producto 
                     WHERE codigo_producto = @codigo 
                        OR nombre = @nombre";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("codigo", codigo);
            cmd.Parameters.AddWithValue("nombre", nombre);
            

            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

    }
}

