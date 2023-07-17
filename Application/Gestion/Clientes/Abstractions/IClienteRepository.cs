using Domain.Clientes;
using Domain.Usuarios;

namespace Application.Gestion.Clientes.Abstractions;
    public interface IClienteRepository
    {
    IQueryable<Usuario> Get();
    Task CreateClienteAsync(Cliente cliente);
    }
