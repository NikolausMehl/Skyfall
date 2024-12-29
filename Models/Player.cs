using System.Text.Json.Serialization;

namespace Skyfall.Models
{
    public record Player(string Name, string GameId)
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonIgnore]
        public bool IsImposter { get; set; }
        [JsonIgnore]
        public string? AssignedWord { get; set; }
        [JsonIgnore]
        public bool HasVoted { get; set; }
        [JsonIgnore]
        public string? VotedForPlayerId { get; set; }
    }
}
