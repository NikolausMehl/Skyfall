using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Skyfall.Services;

namespace Skyfall.Pages;

public partial class Start(ICategoryService CategoryService, IGameService GameService, NavigationManager Navigation, ILocalStorageService LocalStorage)
{
    private Player? Agent;
    private List<Category> Categories = [];
    private string Category = string.Empty;
    private string CodeName = string.Empty;
    private MudForm form = default!;
    private int Rounds = 4;
    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadAgent();
        }
    }

    protected override void OnInitialized()
    {
        Categories = CategoryService.GetCategories();
    }
    private async Task LoadAgent()
    {
        try
        {
            Agent = await LocalStorage.GetItemAsync<Player>(nameof(Agent));
        }
        catch { }

        if ((Agent?.GameId) == null)
        {
            return;
        }

        if (GameService.HasGame(Agent.GameId))
        {
            Navigation.NavigateTo($"/game/{Agent.GameId}");
        }
        else
        {
            CodeName = Agent.Name;
            await InvokeAsync(StateHasChanged);
        }

    }

    private async void StartGame()
    {
        await form.Validate();
        if (!form.IsValid) return;

        Agent = Agent?.Name == CodeName
            ? GameService.CreateGame(Agent, Category, Rounds)
            : GameService.CreateGame(CodeName, Category, Rounds);

        await LocalStorage.SetItemAsync(nameof(Agent), Agent);

        if (GameService.IsDevelopment)
        {
            var agent1 = new Player("James Bond", Agent.GameId);
            GameService.JoinGame(agent1.GameId, agent1);
            var agent2 = new Player("Mr Bean", Agent.GameId);
            GameService.JoinGame(agent1.GameId, agent2);
        }

        Navigation.NavigateTo($"/game/{Agent.GameId}");
    }
}