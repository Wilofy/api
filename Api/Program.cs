using Application.Common.Abstractions;
using Application.Common.Services;
using Application.Gestion.Clientes;
using Application.Gestion.Clientes.Abstractions;
using Application.Gestion.Inventario;
using Application.Gestion.Inventario.Abstractions;
using Application.Gestion.Inventario.Services;
using Application.Gestion.Notifications;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using static Application.Gestion.Notifications.NotificationsHub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(GetClientes).Assembly));
builder.Services.AddScoped<IClienteRepository,ClienteRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddDbContext<DataContext>(opt => 
opt.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});
builder.Services.AddSignalR();

builder.Services.AddHttpClient<MeLiService>("MercadoLibre");
builder.Services.AddHttpClient<TiendaNubeService>("TiendaNube");

builder.Services.AddScoped<MeLiService>();
builder.Services.AddScoped<TiendaNubeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.GetClientesEndpoint();
app.GetProductosEndpoint();
app.PostProductoEndpoint();

app.PostTokenEndpoint();
app.DeleteTokenEndpoint();

app.NotificationsEndpoint();
app.MapHub<HubNotifications>("/myhub");

app.UseCors();

app.Run();