using Blazored.LocalStorage;
using MudBlazor.Services;
using Skyfall.Server;
using Skyfall.Services;

var builder = WebApplication.CreateBuilder(args);
AppState appState = new();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSingleton(provider => appState);
builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    appState.IsDevelopment = true;
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
