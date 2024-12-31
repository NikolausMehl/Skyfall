using Microsoft.AspNetCore.Components;
using MudBlazor;
using Skyfall.Services;

namespace Skyfall.Pages.Components;

public partial class InstructionPanel(IGameService GameService) : IDisposable
{
    [CascadingParameter] public GameState Model { get; set; } = default!;
    protected Player Agent => Model.Agent;
    protected Message? CurrentQuestion { get; private set; }
    protected MudForm Form { get; set; } = default!;
    protected Game Game => Model.Game;

    protected string Instructions =>
    Game.State switch
    {
        GameStates.Recruting => "Waiting for players to join",
        GameStates.Playing => $"Round {Game.CurrentRound}",
        GameStates.Voting => $"Votes: {Game.VoteCount}",
        GameStates.Ended => "Game Over",
        _ => "Unknown"
    };

    protected bool IsMyTurn => Game.State == GameStates.Playing && Game.CurrentPlayer.Id == Agent.Id && Game.Answerer.Length == 0;
    protected string NewMessage { get; set; } = string.Empty;
    protected Player? SelectedPlayer;

    public void Dispose()
    {
        GameService.GameEvent -= OnGameEvent;
        Model.SelectedPlayerChanged -= SelectedPlayerChanged;
        GC.SuppressFinalize(this);
    }

    protected async void AnswerQuestion()
    {
        await Form.Validate();
        if (!Form.IsValid) return;
        if (CurrentQuestion == null) return;
        GameService.Chat(Message.Answer(Agent, CurrentQuestion.SenderId, NewMessage));
        Model.ClearSelectedPlayer();
        NewMessage = "";
    }

    protected async void AskQuestion()
    {
        await Form.Validate();
        if (!Form.IsValid) return;
        if (SelectedPlayer == null) return;
        GameService.Chat(Message.Question(Agent, SelectedPlayer, NewMessage));
        Model.ClearSelectedPlayer();
        NewMessage = "";
    }

    protected override void OnInitialized()
    {
        GameService.GameEvent += OnGameEvent;
        Model.SelectedPlayerChanged += SelectedPlayerChanged;
    }

    private async void SelectedPlayerChanged(object? sender, Player? player)
    {
        SelectedPlayer = player;
        await InvokeAsync(StateHasChanged);
    }

    protected async void SendChatMessage()
    {
        await Form.Validate();
        if (!Form.IsValid) return;
        GameService.Chat(Message.Send(Agent, NewMessage, SelectedPlayer));
        Model.ClearSelectedPlayer();
        NewMessage = "";
    }

    protected void SubmitVote()
    {
        if (SelectedPlayer == null) return;
        GameService.SubmitVote(Game.GameId, SelectedPlayer.Id, Agent.Id);
    }

    private async void OnGameEvent(object? sender, GameEvent e)
    {
        try
        {
            switch (e.Event)
            {
                case GameEvents.AskedQuestion:
                    CurrentQuestion = Game.Chat.Last();
                    break;

                case GameEvents.PlayComplete:
                    CurrentQuestion = null;
                    break;

                case GameEvents.GameEnded:
                    CurrentQuestion = null;
                    Model.ClearSelectedPlayer();
                    break;
            }
            await InvokeAsync(StateHasChanged);
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