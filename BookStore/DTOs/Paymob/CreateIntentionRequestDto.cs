using System.Text.Json.Serialization;

namespace BookStore.DTOs.Paymob
{
    public class CreateIntentionRequestDto
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("payment_methods")]
        public int[] PaymentMethods { get; set; } = Array.Empty<int>();

        [JsonPropertyName("billing_data")]
        public BillingDataDto BillingData { get; set; } = null!;

        [JsonPropertyName("notification_url")]
        public string NotificationUrl { get; set; } = string.Empty;


        [JsonPropertyName("redirection_url")]
        public string RedirectionUrl { get; set; } = string.Empty;


    }
}
