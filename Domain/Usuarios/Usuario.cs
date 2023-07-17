using Domain.Clientes;
using Domain.Inventario;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Usuarios;
    public class Usuario
    {
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    public Empresa? Empresa { get; set; }
    public List<Producto>? Productos { get; set; }
    public List<Cliente>? Clientes { get; set; }

}
