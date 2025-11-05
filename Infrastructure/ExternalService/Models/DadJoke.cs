using System.Text.Json.Serialization;

namespace Infrastructure.ExternalService.Models
{
    public class DadJoke
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("joke")]
        public string Joke { get; set; } = string.Empty;
    }
}
