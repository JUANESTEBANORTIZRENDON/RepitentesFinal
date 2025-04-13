using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TiendaVirtual.Models;

public partial class TiendaDbContext : DbContext
{
    public TiendaDbContext()
    {
    }

    public TiendaDbContext(DbContextOptions<TiendaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Carrito> Carritos { get; set; }

    public virtual DbSet<CarritoProducto> CarritoProductos { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<FacturaDetalle> FacturaDetalles { get; set; }

    public virtual DbSet<HistorialCompra> HistorialCompras { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=ep-blue-hat-a5e7rnyz-pooler.us-east-2.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_klWR3jf6EiHv;Ssl Mode=Require");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.HasKey(e => e.IdCarrito).HasName("carrito_pkey");

            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Carritos).HasConstraintName("carrito_id_usuario_fkey");
        });

        modelBuilder.Entity<CarritoProducto>(entity =>
        {
            entity.HasKey(e => new { e.IdCarrito, e.IdProducto }).HasName("carrito_producto_pkey");

            entity.HasOne(d => d.IdCarritoNavigation).WithMany(p => p.CarritoProductos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("carrito_producto_id_carrito_fkey");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.CarritoProductos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("carrito_producto_id_producto_fkey");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.IdFactura).HasName("factura_pkey");

            entity.Property(e => e.Fecha).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Facturas).HasConstraintName("factura_id_usuario_fkey");
        });

        modelBuilder.Entity<FacturaDetalle>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("factura_detalle_pkey");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.FacturaDetalles).HasConstraintName("factura_detalle_id_factura_fkey");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.FacturaDetalles).HasConstraintName("factura_detalle_id_producto_fkey");
        });

        modelBuilder.Entity<HistorialCompra>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("historial_compra_pkey");

            entity.Property(e => e.Fecha).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.HistorialCompras).HasConstraintName("historial_compra_id_factura_fkey");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialCompras).HasConstraintName("historial_compra_id_usuario_fkey");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("producto_pkey");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("rol_pkey");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("usuario_pkey");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios).HasConstraintName("usuario_id_rol_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
