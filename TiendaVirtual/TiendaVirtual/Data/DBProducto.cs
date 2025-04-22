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
    }
}
