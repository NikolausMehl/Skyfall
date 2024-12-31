using Microsoft.AspNetCore.Components;
using MudBlazor;
using Skyfall.Services;

namespace Skyfall.Pages.Components;

public partial class PlayerList(IGameService gameService, ISnackbar snackbar) : IDisposable
{
    public enum DisplayModes { Bar, List }

    [Parameter, EditorRequired] public DisplayModes DisplayMode { get; set; } = DisplayModes.List;
    [CascadingParameter] public GameState Model { get; set; } = default!;
    protected Player Agent => Model.Agent;
    protected Game Game => Model.Game;

    public void Dispose()
    {
        gameService.GameEvent -= OnGameEvent;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        gameService.GameEvent += OnGameEvent;
    }

    protected void ShowPasscode()
    {
        if (Agent.AssignedWord?.Length > 0)
        {
            snackbar.Add($"Your secret code is {Agent.AssignedWord}");
        }
    }

    protected void ToggleSelected(Player player)
    {
        if (Model.SelectedPlayer?.Id == player.Id)
        {
            Model.ClearSelectedPlayer();
        }
        else
        {
            Model.SelectPlayer(player);
        }
    }

    private async void OnGameEvent(object? sender, GameEvent e)
    {
        try
        {
            switch (e.Event)
            {
                case GameEvents.StartGame:
                    ShowPasscode();
                    break;
                case GameEvents.JoinGame:
                    await InvokeAsync(StateHasChanged);
                    break;
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