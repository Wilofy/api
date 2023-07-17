using Application.Gestion.Clientes.Abstractions;
using Domain.Clientes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;

namespace Application.Gestion.Clientes;
    public static class GetClientes
    {
    public static void GetClientesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/usuario/clientes/{id}", async (ISender mediator, int idUsuario) =>
        {
            return await mediator.Send(new GetClientesQuery(idUsuario));
        })
        .Produces(StatusCodes.Status200OK);
    }

    public record GetClientesQuery(int IdUsuario) : IRequest<IResult>;

    public sealed class GetClientesQueryHandler : IRequestHandler<GetClientesQuery, IResult>
    {
        private readonly IClienteRepository _repo;
        public GetClientesQueryHandler(IClienteRepository _repo)
        {
            this._repo = _repo;
        }
        public async Task<IResult> Handle(GetClientesQuery request, CancellationToken cancellationToken)
        {
            var results = _repo.Get()
                   .Select(u => u.Clientes).ToList();
                   

            return Results.Ok(results);
        }
    }


}
