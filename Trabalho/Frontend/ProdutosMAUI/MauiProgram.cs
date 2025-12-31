using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting; // Para suportar Blazor
using RCLProdutos.Services.Interfaces;
using RCLProdutos.Services;
using RCLAPI;
using RCLAPI.Services;
using ProdutosMAUI.Services;

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

            builder.Services.AddScoped<IAuthStorage, SecureStorageAuthStorage>();
            builder.Services.AddHttpClient<IApiServices, ApiService>(client =>
            {
                client.BaseAddress = new Uri(AppConfig.BaseUrl);
            });

            // Registro do HttpContextAccessor (em MAUI, não é comum usá-lo diretamente, mas se for necessário)
            builder.Services.AddHttpContextAccessor();

            // Registro do AuthService (como no código original)
            builder.Services.AddScoped<AuthService>();

            return builder.Build();
        }
    }
}
