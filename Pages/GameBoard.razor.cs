using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Skyfall.Services;

namespace Skyfall.Pages
{
    public partial class GameBoard(IGameService GameService, ILocalStorageService LocalStorage, NavigationManager Navigation)
    {
        public GameState Model { get; set; } = new();
        protected Game Game => Model.Game;
        protected Player Agent => Model.Agent;
        protected Player? SelectedPlayer => Model.SelectedPlayer;

        [Parameter]
        public string GameId { get; set; } = default!;

        protected string PageTitle { get; set; } = "Mission Briefing";

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
            Model.SetGame(GameService.GetGame(GameId));
        }

        private async Task LoadAgent()
        {
            try
            {
                var agent = await LocalStorage.GetItemAsync<Player>(nameof(Model.Agent));
                if (agent == null || agent.GameId != Model.Game.GameId) throw new InvalidOperationException("Invalid agent");
                agent = Model.Game.Players.FirstOrDefault(p => p.Id == agent.Id) ?? throw new InvalidOperationException("Invalid agent");
                Model.SetAgent(agent);
                GameService.JoinGame(Game.GameId, Model.Agent);
                await InvokeAsync(StateHasChanged);
            }
            catch
            {
                Navigation.NavigateTo("/");
                return;
            }
        }
    }
}