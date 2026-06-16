using System.Text.Json;

namespace BookStore.Services.Implementaion
{
    public class PaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PaymobService(HttpClient httpClient,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> CreateIntention()
        {
            var secretKey =
            _configuration["Paymob:SecretKey"];

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://accept.paymob.com/v1/intention/");

            request.Headers.Add(
                "Authorization",
                $"Token {secretKey}");


            var body = new
            {
                amount = 1000,
                currency = "EGP",

                payment_methods = new[]
    {
        5733305
    },

                items = new[]
    {
        new
        {
            name = "Test Book",
            amount = 1000,
            description = "Book Test",
            quantity = 1
        }
    },

                billing_data = new
                {
                    apartment = "NA",
                    first_name = "Khaled",
                    last_name = "Ahmed",
                    street = "NA",
                    building = "NA",
                    phone_number = "+201000000000",
                    city = "Cairo",
                    country = "EG",
                    email = "test@test.com",
                    floor = "NA",
                    state = "Cairo"
                },

                expiration = 3600
            };
            request.Content = JsonContent.Create(body);
            var response =
    await _httpClient.SendAsync(request);

           


            var json =
        await response.Content.ReadAsStringAsync();

            var document = JsonDocument.Parse(json);

            var clientSecret =
                document.RootElement
                        .GetProperty("client_secret")
                        .GetString();

            return clientSecret!;
        }


        public async Task<string> GetAuthToken()
        {
            var apiKey = _configuration["Paymob:ApiKey"];

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens",
                new
                {
                    api_key = apiKey
                });

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var document = JsonDocument.Parse(json);

            var token = document.RootElement
                                .GetProperty("token")
                                .GetString();

            return token;
        }

    }
}
