using Npgsql;
using System;
using System.Collections.Generic;
using TiendaVirtual.Models;

namespace TiendaVirtual.Data
{
    public static class DBProducto
    {
        // Reemplaza esta cadena con la tuya de Neon.tech
        private static readonly string connectionString = "Host=ep-blue-hat-a5e7rnyz-pooler.us-east-2.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_klWR3jf6EiHv;SSL Mode=Require;Trust Server Certificate=true";

        // ✅ Obtener todos los productos
        public static List<Producto> ObtenerProductos()
        {
            List<Producto> lista = new List<Producto>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM producto ORDER BY id_producto";

                using (var cmd = new NpgsqlCommand(query, conn))
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
                            Imagen = reader["imagen"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        // ✅ Insertar nuevo producto
        public static bool InsertarProducto(Producto producto)
        {
            bool resultado = false;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO producto (nombre, codigo_producto, marca, precio_unitario, stock, imagen)
                                 VALUES (@nombre, @codigo, @marca, @precio, @stock, @imagen)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("nombre", producto.Nombre);
                    cmd.Parameters.AddWithValue("codigo", producto.CodigoProducto);
                    cmd.Parameters.AddWithValue("marca", (object)producto.Marca ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("precio", producto.PrecioUnitario);
                    cmd.Parameters.AddWithValue("stock", producto.Stock);
                    cmd.Parameters.AddWithValue("imagen", (object)producto.Imagen ?? DBNull.Value);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    resultado = filasAfectadas > 0;
                }
            }

            return resultado;
        }

        public static List<Producto> ObtenerProductosFiltrados(string busqueda, int pagina, int tamanoPagina, out int total)
        {
            List<Producto> lista = new List<Producto>();
            total = 0;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Conteo total para paginación
                string countQuery = @"SELECT COUNT(*) FROM producto 
                              WHERE (@busqueda IS NULL OR 
                                     nombre ILIKE '%' || @busqueda || '%' OR 
                                     marca ILIKE '%' || @busqueda || '%' OR 
                                     codigo_producto ILIKE '%' || @busqueda || '%')";

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
                                codigo_producto ILIKE '%' || @busqueda || '%')
                         ORDER BY id_producto
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
                                Imagen = reader["imagen"].ToString()
                            });
                        }
                    }
                }
            }

            return lista;
        }

    }
}
