using Application.Common.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Application.Gestion.Inventario;
public static class DeleteToken
{
    public static void DeleteTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("api/usuario/credentials", (ICacheService _cache, 
             int userId, [FromQuery] string plataforma) =>
        {
            switch (plataforma)
            {
                case var p when p == "MercadoLibre":
                _cache.RemoveData($"CredentialsML={userId}");
                    Console.WriteLine("BORRADO ML");
                    break;

                case var p when p == "TiendaNube":
                _cache.RemoveData($"CredentialsTN={userId}");
                    Console.WriteLine("BORRADO TN");
                    break;

                default:
                    return Results.BadRequest();
            }            
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);
    }

}

