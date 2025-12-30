using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting; // Para suportar Blazor
using RCLProdutos.Services.Interfaces;
using RCLProdutos.Services;
using RCLAPI;
using RCLAPI.Services;
using Blazored.LocalStorage; // Blazored LocalStorage
using System.Net.Http; // Para HttpClient

namespace ProdutosMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            AppConfig.Configure(builder.Configuration);
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView(); // Habilita Blazor WebView

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools(); // Ferramentas de desenvolvimento do Blazor
            builder.Logging.AddDebug();
#endif

            // Registro dos serviços
            builder.Services.AddScoped<ISliderUtilsServices, SliderUtilsServices>();
            builder.Services.AddScoped<ICardsUtilsServices, CardsUtilsServices>();

            builder.Services.AddScoped<IApiServices, ApiService>();

            // Registro do HttpContextAccessor (em MAUI, não é comum usá-lo diretamente, mas se for necessário)
            builder.Services.AddHttpContextAccessor();

            // Registro do AuthService (como no código original)
            builder.Services.AddScoped<AuthService>();

            // Registro do HttpClient
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7213") });

            // Registro do LocalStorage (do Blazored)
            builder.Services.AddBlazoredLocalStorage();

            return builder.Build();
        }
    }
}
