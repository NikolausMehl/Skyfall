namespace Skyfall.Services;

public record AppState
{
    public bool IsDevelopment { get; set; }
    public Dictionary<string, Game> Games { get; set; } = [];
}
