using Microsoft.AspNetCore.Components;
using Skyfall.Services;

namespace Skyfall.Pages.Components;

public partial class MissionStatus(IGameService gameService, NavigationManager navigation) : IDisposable
{
    [CascadingParameter] public GameState Model { get; set; } = default!;
    protected Player Agent => Model.Agent;
    protected Game Game => Model.Game;
    protected bool IsHost => Game.Players[0].Id == Agent.Id;

    protected string Status => Game.State switch
    {
        GameStates.Recruting => $"Recruiting for {Game.CurrentCategory.Name} mission",
        GameStates.Playing => Game.GetNextMove(),
        GameStates.Voting => "Time to identify the imposter",
        GameStates.Ended => $"Game Over {Game.GetOutcome()}",
        _ => "Unknown"
    };

    public void Dispose()
    {
        gameService.GameEvent -= OnGameEvent;
        GC.SuppressFinalize(this);
    }
    protected override void OnInitialized()
    {
        gameService.GameEvent += OnGameEvent;
    }

    private void OnGameEvent(object? sender, GameEvent e)
    {
        try
        {
            switch (e.Event)
            {
                case GameEvents.PlayerRemoved:
                    if (e.PlayerId == Agent.Id)
                    {
                        navigation.NavigateTo("/");
                        return;
                    }
                    break;

                case GameEvents.GameCancelled:
                    navigation.NavigateTo("/");
                    return;
            }
        }
        catch (TaskCanceledException)
        {
            //Ignore
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

    }
}