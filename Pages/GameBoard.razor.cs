using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Skyfall.Services;

namespace Skyfall.Pages
{
    public partial class GameBoard(IGameService GameService, NavigationManager Navigation, ILocalStorageService LocalStorage, IJSRuntime JSRuntime, ISnackbar snackbar) : IDisposable
    {
        private Game Game = default!;
        private Player? SelectedPlayer;

        [Parameter]
        public string GameId { get; set; } = default!;
        public bool IsHost => Game.Players[0].Id == Agent.Id;
        protected string ChatWindowId { get; set; } = $"{nameof(ChatWindowId)}-{Guid.NewGuid()}";

        protected Player Agent { get; set; } = default!;
        protected MudForm Form { get; set; } = default!;
        protected string NewMessage { get; set; } = string.Empty;
        protected string PageTitle { get; set; } = "Mission Briefing";

        protected bool IsMyTurn => Game.State == GameStates.Playing && Game.CurrentPlayer.Id == Agent.Id && Game.Answerer.Length == 0;

        public Message? CurrentQuestion { get; private set; }

        public void Dispose()
        {
            GameService.GameEvent -= OnGameEvent;
            GC.SuppressFinalize(this);
        }

        public void SubmitVote()
        {
            if (SelectedPlayer == null) return;
            GameService.SubmitVote(Game.GameId, SelectedPlayer.Id, Agent.Id);
        }

        public async void AskQuestion()
        {
            await Form.Validate();
            if (!Form.IsValid) return;
            if (SelectedPlayer == null) return;
            GameService.Chat(Message.Question(Agent, SelectedPlayer, NewMessage));
            SelectedPlayer = null;
            NewMessage = "";
        }

        public async void AnswerQuestion()
        {
            await Form.Validate();
            if (!Form.IsValid) return;
            if (CurrentQuestion == null) return;
            GameService.Chat(Message.Answer(Agent, CurrentQuestion.SenderId, NewMessage));
            SelectedPlayer = null;
            NewMessage = "";
        }

        public async void SendChatMessage()
        {
            await Form.Validate();
            if (!Form.IsValid) return;
            GameService.Chat(Message.Send(Agent, NewMessage, SelectedPlayer));
            SelectedPlayer = null;
            NewMessage = "";
        }

        public void ShowPasscode()
        {
            if (Agent.AssignedWord?.Length > 0)
            {
                snackbar.Add($"Your secret code is {Agent.AssignedWord}");
            }
        }

        public void RemovePlayer()
        {
            if (SelectedPlayer == null) return;
            GameService.RemovePlayer(SelectedPlayer);
        }

        public void ToggleSelected(Player player)
        {
            if (SelectedPlayer?.Id == player.Id)
            {
                SelectedPlayer = null;
            }
            else
            {
                SelectedPlayer = player;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadAgent();
            }
        }

        protected override void OnInitialized()
        {
            if (string.IsNullOrEmpty(GameId) || !GameService.HasGame(GameId))
            {
                Navigation.NavigateTo("/");
                return;
            }
            Game = GameService.GetGame(GameId);
            GameService.GameEvent += OnGameEvent;
        }

        private async Task CopyJoinLink()
        {
            var link = GenerateJoinLink();
            await JSRuntime.InvokeVoidAsync("copyToClipboard", link);
            snackbar.Add("Join link has been copied to your clipboard");
        }

        private string GenerateJoinLink()
        {
            var absoluteUri = Navigation.ToAbsoluteUri($"/join/{GameId}").ToString();
            return absoluteUri;
        }

        private async Task LoadAgent()
        {
            try
            {
                var agent = await LocalStorage.GetItemAsync<Player>(nameof(Agent));
                if (agent == null || agent.GameId != Game.GameId) throw new InvalidOperationException("Invalid agent");
                Agent = Game.Players.FirstOrDefault(p => p.Id == agent.Id) ?? throw new InvalidOperationException("Invalid agent");
                await InvokeAsync(StateHasChanged);
            }
            catch
            {
                Navigation.NavigateTo("/");
                return;
            }
        }


        private void StartGame()
        {
            GameService.StartGame(Game.GameId);
            NewMessage = "Lets play";
            SendChatMessage();
        }
        private void RestartGame()
        {
            GameService.RestartGame(Game.GameId);
        }

        private void EndGame()
        {
            GameService.EndGame(Game.GameId);
        }

        private void CancelGame()
        {
            GameService.CancelGame(Game.GameId);
        }

        private void SkipTurn()
        {
            GameService.NextTurn(Game.GameId);
        }

        private async void OnGameEvent(Game sender, GameEvent e)
        {
            switch (e.Event)
            {
                case GameEvents.StartGame:
                    ShowPasscode();
                    break;

                case GameEvents.PlayerRemoved:
                    if (e.PlayerId == Agent.Id)
                    {
                        Navigation.NavigateTo("/");
                        return;
                    }
                    break;

                case GameEvents.GameCancelled:
                    Navigation.NavigateTo("/");
                    return;

                case GameEvents.PlayComplete:
                    CurrentQuestion = null;
                    break;

                case GameEvents.GameEnded:
                    CurrentQuestion = null;
                    SelectedPlayer = null;
                    break;

                case GameEvents.AskedQuestion:
                    CurrentQuestion = Game.Chat.Last();
                    break;
            }

            await InvokeAsync(StateHasChanged);
            await JSRuntime.InvokeVoidAsync("scrollToBottom", ChatWindowId);

        }
    }
}