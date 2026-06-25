using System.Text.Json.Serialization;

namespace BookStore.DTOs.Paymob
{
    public class PaymobOrderDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}
