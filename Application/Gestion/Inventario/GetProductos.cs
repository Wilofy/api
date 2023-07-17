using Application.Common.Abstractions;
using Application.Gestion.Inventario.Abstractions;
using Application.Gestion.Inventario.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Application.Gestion.Inventario;
public static class GetProductos
{
    public static void GetProductosEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/usuario/productos", async (ISender mediator, [FromBody] List<CredentialsRequest> credentialsRequest) =>
        {
            return await mediator.Send(new GetProductosQuery(credentialsRequest));
        })
        .Produces(StatusCodes.Status200OK);
    }

    public record GetProductosQuery(List<CredentialsRequest> CredentialsRequest) : IRequest<IResult>;

    public sealed class GetProductosQueryHandler : IRequestHandler<GetProductosQuery, IResult>
    {
        private readonly IProductoRepository _repo;
        private readonly ICacheService _cache;
        private readonly MeLiService _meLiService;
        private readonly TiendaNubeService _tnService;

        public GetProductosQueryHandler(IProductoRepository repo, ICacheService cache,
            MeLiService meLiService,
            TiendaNubeService tnService)
        {
            _repo = repo;
            _cache = cache;
            _meLiService = meLiService;
            _tnService = tnService;
        }
        public async Task<IResult> Handle(GetProductosQuery request, CancellationToken cancellationToken)
        {
            //var results = _repo.Get().ToList();

            //var cache = _cache.GetData<IEnumerable<ProductoResponse>>("productos");

            //if (cache != null && cache.Count() > 0)
            //{
            //    return Results.Ok(cache);
            //}

            //var expireTime = DateTimeOffset.Now.AddMinutes(60);

            var tiendaN = request.CredentialsRequest.FirstOrDefault(x => x.Plataforma == "TiendaNube");
            var meLi = request.CredentialsRequest.FirstOrDefault(x => x.Plataforma == "Mercado Libre");

            var cacheTN = tiendaN is not null ? _cache.GetData<string>($"CredentialsTN={tiendaN.AppId}") : null;
            var cacheML = meLi is not null ? _cache.GetData<CredentialsML>($"CredentialsML={meLi.AppId}") : null;

            var productos = new List<ProductoResponse>();

            Task<IReadOnlyCollection<ProductoTiendaNube>>? productosTn = null;
            Task<IReadOnlyCollection<ProductoMercadoLibre>>? productosMl = null;

            List<Task> tasks = new();
            if (tiendaN != null && cacheTN != null)
            {
                productosTn = _tnService.GetProductosAsync(tiendaN.AppId);
                tasks.Add(productosTn);
            }

            if (meLi != null && cacheML != null)
            {
                productosMl = _meLiService.GetProductosAsync(meLi.AppId,5, 0);
                tasks.Add(productosMl);
            }

            await Task.WhenAll(tasks);

            if (productosTn != null && productosTn.Result.Count > 0)
            {
                foreach (var item in await productosTn)
                {
                    productos.Add(
                        new(
                            Name: item.Name.Es,
                            Price: decimal.Parse(item.Variants[0].Price),
                            Platform: "Tienda Nube",
                            Image: item.Images[0].Src,
                            FechaCreado: DateTime.Parse(item.Created_at)
                            ));
                }
            }

            if (productosMl != null && productosMl.Result.Count > 0)
            {
                foreach (var item in await productosMl)
                {
                    productos.Add(
                        new(
                            Name: item.Body.Title,
                            Price: item.Body.Price,
                            Platform: "Mercado Libre",
                            Image: item.Body.Pictures[0].Secure_url,
                            FechaCreado: item.Body.Date_created
                            ));
                }
            }
            //_cache.SetData<IEnumerable<ProductoResponse>>("productos", productos, expireTime);

            return Results.Ok(productos.OrderByDescending(p => p.FechaCreado));
        }
    }

    public record ProductoResponse(
        string Name,
        decimal Price,
        string Platform,
        string Image,
        DateTime FechaCreado);
}

public record CredentialsRequest(
     string Plataforma,
     int AppId
    );
