using Application.Common.Abstractions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static Application.Gestion.Inventario.PostProducto;

namespace Application.Gestion.Inventario.Services;
public class TiendaNubeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cache;

    public TiendaNubeService(IHttpClientFactory httpClientFactory, ICacheService cache)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<ProductoTiendaNube>> GetProductosAsync(
        int appId)
    {
        var cache = _cache.GetData<string>($"CredentialsTN={appId}");
        var products = Array.Empty<ProductoTiendaNube>();

        if (cache != null)
        {
            var _bearerToken = cache;

            var client = _httpClientFactory.CreateClient("TiendaNube");
            client.DefaultRequestHeaders.Add("Authentication", $"bearer {_bearerToken}");
            client.DefaultRequestHeaders.Add("User-Agent", "Wilofy (wilofy@outlook.com)");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var temp = await client.GetFromJsonAsync<IEnumerable<ProductoTiendaNube>>
                  ($"https://api.tiendanube.com/v1/{appId}" +
                   $"/products?fields=id,name,free_shipping,variants,images,created_at", options)
                      ?? Enumerable.Empty<ProductoTiendaNube>();

                products = temp.ToArray();

            }
            catch (Exception ex) { }
        }

        return products;
    }

    public async Task<HttpResponseMessage> PostProductoAsync(
        int appId,
        ProductoRequest p)
    {
        var bearerToken = _cache.GetData<string>($"CredentialsTN={appId}");

        var client = _httpClientFactory.CreateClient("TiendaNube");
        client.DefaultRequestHeaders.Add("Authentication", bearerToken);
        client.DefaultRequestHeaders.Add("User-Agent", "Wilofy (wilofy@outlook.com)");

        var data = new
        {
            images = new[] { new { src = p.UrlImagen } },
            name = new { es = p.Titulo },
            variants = new[]
            {
              new
              {
                price = p.Precio,
                stock_management = true,
                stock = p.Stock
              }
            }
        };

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return await client.PostAsync($"https://api.tiendanube.com/v1/{appId}/products", content);
    }


    public async Task<string?> PostToken(string code, int appId)
    {
        var client = _httpClientFactory.CreateClient("TiendaNube");
        var url = "https://www.tiendanube.com/apps/authorize/token";

        var payload = new
        {
            client_id = "7229",
            client_secret = "9c096a605f99e7120411b0892a248b9e9947e5dcefb99971",
            grant_type = "authorization_code",
            code = code
        };

        var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);
        TokenResponseTN? responseContent = await response.Content.ReadFromJsonAsync<TokenResponseTN>();

        if (responseContent!.Access_token != null)
        {
            _cache.SetData<string>($"CredentialsTN={appId}", responseContent.Access_token);
        }

        return responseContent.Access_token;
    }

}
public class TokenResponseTN
{
    public string? Access_token { get; set; }
    public string? Token_type { get; set; }
    public string? Scope { get; set; }
    public int User_id { get; set; }
}
public record ProductoTiendaNube(
    int Id,
    Name Name,
    bool Free_Shipping,
    string Created_at,
    List<Variant> Variants,
    List<Image> Images);

public record Name(string Es);
public record Variant(
    string Price,
    string PromotionalPrice,
    int Stock);

public record Image(string Src);

