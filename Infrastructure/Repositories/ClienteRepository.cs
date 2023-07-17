using Application.Gestion.Clientes.Abstractions;
using Domain.Clientes;
using Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
public class ClienteRepository : IClienteRepository
{
    private readonly DataContext _context;
    public ClienteRepository(DataContext context)
    {
        _context = context;
    }
    public IQueryable<Usuario> Get()
    => _context.Usuarios;
            

    public async Task CreateClienteAsync(Cliente cliente)
    {
       await _context.AddAsync(cliente);
    }
}
