using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class DadJoke
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("joke")]
        public string Joke { get; set; } = string.Empty;
    }
}
