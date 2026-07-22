using System.Text.Json.Serialization;

namespace BookStore.DTOs.Paymob
{
    public class BillingDataDto
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("street")]
        public string Address { get; set; } = string.Empty;
    }
}
