using System.Text.Json.Serialization;

namespace BookStore.DTOs.Paymob
{
    public class CreateIntentionRequestDto
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("payment_methods")]
        public int[] PaymentMethods { get; set; }

        [JsonPropertyName("billing_data")]
        public BillingDataDto BillingData { get; set; }

        [JsonPropertyName("notification_url")]
        public string NotificationUrl { get; set; }


        [JsonPropertyName("redirection_url")]
        public string RedirectionUrl { get; set; }


    }
}
