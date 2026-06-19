namespace BookStore.DTOs.Paymob
{
    public class PaymobTransactionDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("amount")]
        public long Amount { get; set; }
    }
}
