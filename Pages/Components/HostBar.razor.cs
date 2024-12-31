using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Skyfall.Services;

namespace Skyfall.Pages.Components;

public partial class HostBar(IGameService gameService, IJSRuntime jsRuntime, ISnackbar snackbar, NavigationManager navigation) : IDisposable
{
    protected Player? SelectedPlayer;
    [CascadingParameter] public GameState Model { get; set; } = default!;
    protected Player Agent => Model.Agent;
    protected Game Game => Model.Game;
    public void Dispose()
    {
        Model.SelectedPlayerChanged -= SelectedPlayerChanged;
        GC.SuppressFinalize(this);
    }

    protected void CancelGame() => gameService.CancelGame(Game.GameId);

    protected async Task CopyJoinLink()
    {
        var link = GenerateJoinLink();
        await jsRuntime.InvokeVoidAsync("copyToClipboard", link);
        snackbar.Add("Join link has been copied to your clipboard");
    }

    protected void EndGame() => gameService.EndGame(Game.GameId);
    protected override void OnInitialized()
    {
        Model.SelectedPlayerChanged += SelectedPlayerChanged;
    }

    protected void RemovePlayer()
    {
        if (SelectedPlayer == null) return;
        gameService.RemovePlayer(SelectedPlayer);
    }
    protected void RestartGame() => gameService.RestartGame(Game.GameId);
    protected void SkipTurn() => gameService.NextTurn(Game.GameId);
    protected void StartGame() => gameService.StartGame(Game.GameId);
    private string GenerateJoinLink()
    {
        var absoluteUri = navigation.ToAbsoluteUri($"/join/{Game.GameId}").ToString();
        return absoluteUri;
    }

    private async void SelectedPlayerChanged(object? sender, Player? player)
    {
        SelectedPlayer = player;
        await InvokeAsync(StateHasChanged);
    }
}