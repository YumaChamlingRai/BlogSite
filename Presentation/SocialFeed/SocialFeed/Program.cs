using Bislerium;
using Bislerium.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var services = builder.Services;

var rootComponent = builder.RootComponents;

rootComponent.Add<App>("#app");

rootComponent.Add<HeadOutlet>("head::after");

services.AddScoped(x => new HttpClient
{
    BaseAddress = new Uri("https://localhost:44340/")
});

services.AddTransient<APIService>();

services.AddBlazoredLocalStorage();

services.AddAuthorizationCore();

services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

var app = builder.Build();

await app.RunAsync();