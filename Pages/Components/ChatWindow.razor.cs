using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Skyfall.Services;

namespace Skyfall.Pages.Components;

public partial class ChatWindow(IGameService gameService, IJSRuntime jsRuntime) : IDisposable
{
    [CascadingParameter] public GameState Model { get; set; } = default!;
    protected Player Agent => Model.Agent;
    protected string ChatWindowId { get; set; } = $"{nameof(ChatWindowId)}-{Guid.NewGuid()}";
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

    private async void OnGameEvent(object? sender, GameEvent e)
    {
        try
        {
            switch (e.Event)
            {
                case GameEvents.AnsweredQuestion:
                case GameEvents.AskedQuestion:
                case GameEvents.SentMessage:
                    await InvokeAsync(StateHasChanged);
                    await jsRuntime.InvokeVoidAsync("scrollToBottom", ChatWindowId);
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