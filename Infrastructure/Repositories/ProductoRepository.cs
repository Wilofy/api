using Application.Gestion.Inventario.Abstractions;
using Domain.Inventario;

namespace Infrastructure.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly DataContext _context;
    public ProductoRepository(DataContext context)
    {
        _context = context;
    }
    public IQueryable<Producto> Get()
    => _context.Productos;


    public async Task CreateProductoAsync(Producto producto)
    {
        await _context.Productos.AddAsync(producto);
    }
}
