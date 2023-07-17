using Domain.Inventario;

namespace Application.Gestion.Inventario.Abstractions;
    public interface IProductoRepository
    {
    IQueryable<Producto> Get();

    Task CreateProductoAsync(Producto producto);
    }
