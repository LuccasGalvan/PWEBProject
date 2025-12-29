using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using ProdutosBlazor.Components;

using RCLProdutos.Services.Interfaces;
using RCLProdutos.Services;
using RCLAPI.Services;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

builder.Services.AddScoped<ISliderUtilsServices, SliderUtilsServices>();
builder.Services.AddScoped<ICardsUtilsServices, CardsUtilsServices>();

// Registra o serviço IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IApiServices, ApiService>();

builder.Services.AddScoped<AuthService>(); 

builder.Services.AddScoped(sp => new HttpClient { BaseAddress =
    new Uri("https://localhost:7213") });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
