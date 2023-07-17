namespace Domain.Clientes;

public class Cliente
{
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    public required string Telefono { get; set; }
    public required Direccion Direccion { get; set; }
    public string? Observaciones { get; set; }
}
