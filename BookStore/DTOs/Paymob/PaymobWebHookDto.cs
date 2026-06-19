namespace BookStore.DTOs.Paymob
{
    public class PaymobWebHookDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("obj")]
        public PaymobTransactionDto Obj { get; set; }
    }
}
