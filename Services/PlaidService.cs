using System.Net.Http.Json;
using System.Text.Json;

public class PlaidService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly string _apiKey;
    private readonly string _apiSecret;




    public PlaidService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
        _apiKey = _config["Plaid:ClientId"] ?? "default_if_missing";
        _apiSecret = _config["Plaid:Secret"] ?? "default_if_missing";

    }

    public async Task<string> ExchangePublicToken(string publicToken)
    {

        Console.WriteLine($"Exchanging public token: {publicToken}");
        var body = new
        {
            client_id = _apiKey,
            secret = _apiSecret,
            public_token = publicToken
        };

        var response = await _http.PostAsJsonAsync("https://sandbox.plaid.com/item/public_token/exchange", body);
        return await response.Content.ReadAsStringAsync(); // You can parse JSON here too
    }

    public async Task<string> CreateLinkToken()
    {
        var webhookUrl = $"budgetbuddy-fxg4g3ccbbe2buet.centralus-01.azurewebsites.net";
        var request = new
        {
            client_id = _apiKey,
            secret = _apiSecret,
            user = new { client_user_id = "user-123" },
            client_name = "Finance Tracker",
            products = new[] { "transactions" },
            country_codes = new[] { "US" },
            language = "en",
            webhook = webhookUrl
        };

        var response = await _http.PostAsJsonAsync($"https://sandbox.plaid.com/link/token/create", request);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetTransactions(string accessToken, DateTime startDate, DateTime endDate)
    {
        var body = new
        {
            client_id = _apiKey,
            secret = _apiSecret,
            access_token = accessToken,
            start_date = startDate.ToString("yyyy-MM-dd"),
            end_date = endDate.ToString("yyyy-MM-dd")
        };

        var response = await _http.PostAsJsonAsync("https://sandbox.plaid.com/transactions/get", body);
        return await response.Content.ReadAsStringAsync(); // or deserialize to a model
    }


    private async Task<string?> GetNgrokPublicUrl()
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync("http://127.0.0.1:4040/api/tunnels");
            var json = JsonDocument.Parse(response);
            var tunnels = json.RootElement.GetProperty("tunnels");

            foreach (var tunnel in tunnels.EnumerateArray())
            {
                if (tunnel.GetProperty("proto").GetString() == "https")
                {
                    return tunnel.GetProperty("public_url").GetString();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not get ngrok URL: " + ex.Message);
        }

        return null;
    }


}
