using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UnitedNationsTelegram.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient()
{
    BaseAddress = new Uri(builder.Configuration["BaseAddress"])
});


await builder.Build().RunAsync();
//https://brianlagunas.com/using-npm-packages-in-blazor/#:~:text=%20Using%20NPM%20Packages%20in%20Blazor%20%201,is%20setup%20properly%20to%20use%20npm%2C...%20More%20