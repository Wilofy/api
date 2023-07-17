using System.Net.Http.Json;
using static Application.Gestion.Inventario.PostProducto;
using System.Text;
using Application.Common.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Application.Gestion.Inventario.Services;
public class MeLiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cache;

    public MeLiService(IHttpClientFactory httpClientFactory, ICacheService cache)
    {
        _httpClientFactory = httpClientFactory; 
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<ProductoMercadoLibre>> GetProductosAsync(int userId,
                                                                    int limit, int offset)
    {
        var cache = _cache.GetData<CredentialsML>($"CredentialsML={userId}");
        var products = Array.Empty<ProductoMercadoLibre>();

        if (cache != null)
        {
            var client = _httpClientFactory.CreateClient("MercadoLibre");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {cache.Access_token}");

            ResultsResponseMeLi results = new();

            try
            {
                results = await client.GetFromJsonAsync<ResultsResponseMeLi>
                  ($"https://api.mercadolibre.com/users/{userId}" +
                    $"/items/search?attributes=results&limit={limit}&offset={offset}" +
                    $"&orders=start_time_desc") ?? new ResultsResponseMeLi();
            }
            catch (Exception e)
            {
                RefreshTokenResponse meliRefresh = await PostRefreshTokenAsync(cache.Refresh_token);

                if (meliRefresh.Access_token != null)
                {
                    client.DefaultRequestHeaders.Remove("Authorization");
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {meliRefresh.Access_token}");

                    try
                    {
                        results = await client.GetFromJsonAsync<ResultsResponseMeLi>
                          ($"https://api.mercadolibre.com/users/{userId}" +
                            $"/items/search?attributes=results&limit={limit}&offset={offset}" +
                            $"&orders=start_time_desc") ?? new ResultsResponseMeLi();
                    }
                    catch
                    {
                        return products;
                    }
                }
            }

            if (results.Results!.Count() > 0)
            {
                StringBuilder sb = new();
                foreach (var item in results.Results)
                {
                    sb.Append($"{item},");
                }

                var temp = await client.GetFromJsonAsync<IEnumerable<ProductoMercadoLibre>>
                           ($"https://api.mercadolibre.com/items?ids={sb}&attributes=" +
                           $"title,price,base_price,initial_quantity,available_quantity," +
                           $"sold_quantity,condition,permalink,status,pictures,date_created")
                                ?? Enumerable.Empty<ProductoMercadoLibre>();

                products = temp.ToArray();                   
            }
        }
        return products;
    }

    public async Task<HttpResponseMessage> PostProductoAsync(int userId, ProductoRequest p)
    {
        var cache = _cache.GetData<CredentialsML>($"CredentialsML={userId}");
        var result = new HttpResponseMessage();

        if (cache != null)
        {
            var data = new
            {
                title = p.Titulo,
                category_id = "MLA3530",
                price = p.Precio,
                currency_id = "ARS",
                available_quantity = p.Stock,
                buying_mode = "buy_it_now",
                condition = p.Condicion.ToLower() switch
                {
                    "new" => "new",
                    "used" => "used",
                    _ => "new"
                },
                listing_type_id = p.TipoPublicacion.ToLower() switch
                {
                    "bronze" => "bronze",
                    "silver" => "silver",
                    "gold_special" => "gold_special",
                    _ => "bronze"
                },
                sale_terms = new[]
                {
                new { id = "WARRANTY_TYPE", value_name = "Garantía del vendedor" },
                new { id = "WARRANTY_TIME", value_name = "90 días" }
                },
                pictures = new[]
                {
                new { source = p.UrlImagen }
                },
                attributes = new[]
                {
                new { id = "BRAND", value_name = "Marca del producto" },
                new { id = "MODEL", value_name = "Modelo del producto" }
                },
                variations = p.Variations.Select(v => 
                new
                {
                    price = p.Variations[0].Price,
                    available_quantity = v.Available_Quantity,
                    attribute_combinations = new[]
                    {
                      new
                      {
                      name = v.Name,
                      value_name = v.Value_Name
                      }
                    },
                    picture_ids = v.Pictures.ToList()           
                }).ToList()
                //shipping = new
                //{
                //    mode = "me2",
                //    local_pick_up = false,
                //    free_shipping = true,
                //    dimensions = "10x10x10,500",
                //    tags = new[]
                //    {
                //      "fulfillment",
                //      "mandatory_free_shipping"
                //    }
                //}
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient("MercadoLibre");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {cache.Access_token}");
            result = await client.PostAsync("https://api.mercadolibre.com/items", content);

            Console.Write(await result.Content.ReadAsStringAsync());

            if (result.StatusCode == HttpStatusCode.GatewayTimeout || result.StatusCode == HttpStatusCode.InternalServerError)
            {
               await Task.Delay(100);
               result = await client.PostAsync("https://api.mercadolibre.com/items", content);
            }

            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
               RefreshTokenResponse meliRefresh = await PostRefreshTokenAsync(cache.Refresh_token);
                
               if (meliRefresh.Access_token != null)
               {
                await Task.Delay(100);
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {meliRefresh.Access_token}");
                result = await client.PostAsync("https://api.mercadolibre.com/items", content);
              }
              else { return new HttpResponseMessage(HttpStatusCode.BadRequest); }
            }
        }

        return result;
    }

    public async Task<string?> PostToken(string Code, int userId)
    {
        var client = _httpClientFactory.CreateClient("MercadoLibre");
        var url = "https://api.mercadolibre.com/oauth/token";

        var payload = new
        {
            client_id = "2995853406616346",
            client_secret = "KZcnn2RumrOKH9pEFiMkXfezBN75WUcl",
            grant_type = "authorization_code",
            redirect_uri = "http://localhost:4200/home",
            code = Code
        };

        var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);
        TokenResponse? responseContent = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if (responseContent!.Access_token != null)         
        {
            var credentials = new CredentialsML(responseContent.Access_token,
                                                responseContent.User_id,
                                                responseContent.Refresh_token!);

            _cache.SetData<CredentialsML>($"CredentialsML={userId}", credentials);
        }

        return responseContent.Access_token;
    }

    public async Task<RefreshTokenResponse> PostRefreshTokenAsync(string refresh_token)
    {
        var client = _httpClientFactory.CreateClient("MercadoLibre");
        string tokenUrl = "https://api.mercadolibre.com/oauth/token";
        string grantType = "refresh_token";
        string clientId = "2995853406616346";
        string clientSecret = "KZcnn2RumrOKH9pEFiMkXfezBN75WUcl";
        string refreshToken = refresh_token;

            var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", grantType },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "refresh_token", refreshToken }
            });

            var response = await client.PostAsync(tokenUrl, tokenContent);

            RefreshTokenResponse tokenResponse = new();

            if (response.IsSuccessStatusCode)
            {
               tokenResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>()
                                     ?? new RefreshTokenResponse();

            var UpdateCredentials = new CredentialsML(tokenResponse.Access_token,
                                          tokenResponse.User_id,
                                          tokenResponse.Refresh_token);

            _cache.SetData<CredentialsML>($"CredentialsML={tokenResponse.User_id}", UpdateCredentials);

            }

        return tokenResponse;
    }
}

public record RefreshTokenResponse
{
    public string? Access_token { get; set; }
    public int User_id { get; set; }
    public string? Refresh_token { get; set; }
}
public record TokenResponse(
     string Access_token,
     int User_id,
     string Refresh_token);
public record CredentialsML(
    string Access_token,
    int User_id,
    string Refresh_token);
public record ResultsResponseMeLi
{
    public string[]? Results { get; set; }
}
public record ProductoMercadoLibre
{
    public int Code { get; set; }
    public Body? Body { get; set; }
}
public record Body
{
    public string? Title { get; set; }
    public decimal Price { get; set; }
    public List<Picture>? Pictures { get; set; }
    public DateTime Date_created { get; set; }
}
public record Picture(string? Secure_url);
        
