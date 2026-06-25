using System.Text.Json.Serialization;

namespace BookStore.DTOs.Paymob
{
    public class PaymobWebHookDto
    {
        [JsonPropertyName("obj")]
        public PaymobTransactionDto Obj { get; set; }
    }
}
