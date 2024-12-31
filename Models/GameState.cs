

namespace Skyfall.Models;

public class GameState()
{
    public Player Agent { get; private set; } = default!;
    public Game Game { get; private set; } = default!;
    public Player? SelectedPlayer { get; private set; }

    public event EventHandler<Player?>? SelectedPlayerChanged;

    internal void ClearSelectedPlayer()
    {
        SelectedPlayer = null;
        SelectedPlayerChanged?.Invoke(this, null);
    }

    internal void SelectPlayer(Player player)
    {
        SelectedPlayer = player;
        SelectedPlayerChanged?.Invoke(this, player);
    }

    internal void SetAgent(Player agent) => Agent = agent;

    internal void SetGame(Game game) => Game = game;
}
