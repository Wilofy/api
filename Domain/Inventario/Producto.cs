using Domain.Clientes;
using Domain.Usuarios;

namespace Domain.Inventario;
    public class Producto
    {
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string SKU { get; set; }
    public string? Descripcion { get; set; }
    public int Stock { get; set; } = 0;
    public required decimal ImporteCompra { get; set; }
    public required decimal PrecioVenta { get; set; }
    public int UsuarioId { get; set; }
    public required Usuario Usuario { get; set; }

    public Producto()
    {
        Stock++;
    }

}
