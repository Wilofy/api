using Application.Gestion.Inventario.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Application.Gestion.Inventario;
public static class PostProducto
{
    public static void PostProductoEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/usuario/producto", async (ISender mediator, [FromBody] ProductoRequest producto) =>
        {
            return await mediator.Send(new PostProductoCommand(producto));
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }

    public record PostProductoCommand(ProductoRequest Producto) : IRequest<IResult>;

    public sealed class PostProductoCommandHandler : IRequestHandler<PostProductoCommand, IResult>
    {
        private readonly MeLiService _meLiService;
        private readonly TiendaNubeService _tnService;
        public PostProductoCommandHandler(MeLiService meLiService, TiendaNubeService tnService)
        {
            _meLiService = meLiService;
            _tnService = tnService;

        }
        public async Task<IResult> Handle(PostProductoCommand request, CancellationToken cancellationToken)
        {
            var tiendaNubeResponse = _tnService.PostProductoAsync
                (3236869, request.Producto);
            var meLiResponse = _meLiService.PostProductoAsync
                (1385320447, request.Producto);

            await Task.WhenAll(meLiResponse, tiendaNubeResponse);

            var messages = new Dictionary<string, int>
            {
                { "Mercado Libre", (int)meLiResponse.Result.StatusCode },
                { "TiendaNube", (int)tiendaNubeResponse.Result.StatusCode }
            };

            if (!tiendaNubeResponse.Result.IsSuccessStatusCode && !meLiResponse.Result.IsSuccessStatusCode)
                return Results.BadRequest(messages);

            return Results.Ok(messages);
        }
    }

    public record ProductoRequest(
        string Titulo,
        decimal Precio,
        int Stock,
        string UrlImagen,
        //string? CategoriaId,
        //bool? MercadoPago,
        //bool? Free_Shipping,
        //bool? Local_pick_up,
        string TipoPublicacion,
        string Condicion,
        Variation[] Variations
        );

    public record Variation(
         double Price,
         int Available_Quantity,
         string Name,
         string Value_Name,
         string[] Pictures
        );
    
}

