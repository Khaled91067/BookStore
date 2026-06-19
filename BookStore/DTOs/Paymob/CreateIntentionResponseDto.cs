using System.Text.Json.Serialization;

namespace BookStore.DTOs.Paymob
{
    public class CreateIntentionResponseDto
    {
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }
        [JsonPropertyName("intention_order_id")]
        public long IntentionOrderId { get; set; }
    }
}
