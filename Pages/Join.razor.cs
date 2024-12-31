using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Skyfall.Services;

namespace Skyfall.Pages
{
    public partial class Join(IGameService GameService, NavigationManager Navigation, ILocalStorageService LocalStorage, ISnackbar Snackbar)
    {
        private string CodeName = string.Empty;
        private MudForm form = default!;
        private Game Game = default!;

        [Parameter]
        public string GameId { get; set; } = default!;

        protected Player? Agent { get; set; } = default!;
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
        }

        private async void JoinGame()
        {
            await form.Validate();
            if (!form.IsValid) return;

            try
            {
                Agent = Agent?.Name == CodeName
                    ? GameService.JoinGame(Game.GameId, Agent)
                    : GameService.JoinGame(Game.GameId, CodeName);
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message);
                return;
            }

            await LocalStorage.SetItemAsync(nameof(Agent), Agent);
            Navigation.NavigateTo($"/game/{Agent.GameId}");
        }

        private async Task LoadAgent()
        {
            try
            {
                Agent = await LocalStorage.GetItemAsync<Player>(nameof(Agent));
            }
            catch { }

            CodeName = Agent?.Name ?? "";
            await InvokeAsync(StateHasChanged);
        }
    }
}