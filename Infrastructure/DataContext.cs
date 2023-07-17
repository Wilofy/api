using Domain.Clientes;
using Domain.Inventario;
using Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;
public class DataContext : DbContext
{ 
    public DataContext(DbContextOptions<DataContext> options): base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(u =>
                    u.Ignore(e => e.Clientes));

        modelBuilder.Entity<Usuario>().OwnsOne(u => u.Empresa);

        modelBuilder.Entity<Usuario>()
         .Property(b => b.Clientes)
         .HasColumnType("jsonb");

        modelBuilder.Entity<Usuario>().HasMany(u => u.Productos)
                                      .WithOne(p => p.Usuario);

    }

    public DbSet<Usuario> Usuarios {  get; set; }
    public DbSet<Producto> Productos { get; set; }
}
