using AnimalShelterApp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AnimalShelterApp.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure the HttpClient for the app
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register FirestoreService
builder.Services.AddScoped<FirestoreService>();

// ** START: Added Authentication Services **
// Add core authorization services
builder.Services.AddAuthorizationCore();

// Register our custom AuthService as the implementation for AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, AuthService>();

// Register AuthService so it can be injected into other components/services
builder.Services.AddScoped<AuthService>(sp => (AuthService)sp.GetRequiredService<AuthenticationStateProvider>());
// ** END: Added Authentication Services **

await builder.Build().RunAsync();


