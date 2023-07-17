using Application.Gestion.Inventario.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Application.Gestion.Inventario;
public static class PostToken
{
    public static void PostTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/usuario/credentials", async (ISender mediator, [FromBody] TokenRequest credentials) =>
        {
            return await mediator.Send(new PostTokenCommand(credentials));
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }

    public record PostTokenCommand(TokenRequest Credentials) : IRequest<IResult>;

    public sealed class PostTokenCommandHandler : IRequestHandler<PostTokenCommand, IResult>
    {
        private readonly TiendaNubeService _tiendaNubeService;
        private readonly MeLiService _meLiService;         
        public PostTokenCommandHandler(TiendaNubeService tiendaNubeService,
            MeLiService meLiService)
        {
            _tiendaNubeService = tiendaNubeService;
            _meLiService = meLiService;

        }
        public async Task<IResult> Handle(PostTokenCommand request, CancellationToken cancellationToken)
        {
            string? tokenTiendaNube = null;
            string? tokenMeLi = null;

            switch (request.Credentials.Code)
            {
                case var code when code.StartsWith("TG"):
                tokenMeLi = await _meLiService.PostToken(request.Credentials.Code,
                                                         request.Credentials.AppId);
                return tokenMeLi is not null ? Results.Ok() : Results.BadRequest();
                  
                default:
                tokenTiendaNube = await _tiendaNubeService.PostToken(request.Credentials.Code,
                                                                     request.Credentials.AppId);
                return tokenTiendaNube is not null ? Results.Ok() : Results.BadRequest();
            }
        }
    }

    public record TokenRequest(
        string Code,
        int AppId
        );
}

