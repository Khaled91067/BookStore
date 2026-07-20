using System.Text.Json.Serialization;

namespace BookStore.DTOs.Paymob
{
    public class PaymobTransactionDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("order")]
        public PaymobOrderDto Order { get; set; }

        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }
    }
}
