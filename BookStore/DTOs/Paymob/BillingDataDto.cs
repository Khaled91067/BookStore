using System.Text.Json.Serialization;

namespace BookStore.DTOs.Paymob
{
    public class BillingDataDto
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("street")]
        public string Address { get; set; }
    }
}
