using Microsoft.Extensions.Configuration;
using System;

namespace RCLAPI;
public static class AppConfig
{
    public const string ApiBaseUrlConfigKey = "Api:BaseUrl";

    public static string BaseUrl { get; private set; } = string.Empty;

    public static void Configure(IConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var baseUrl = configuration[ApiBaseUrlConfigKey];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException($"Missing configuration value for '{ApiBaseUrlConfigKey}'.");
        }

        BaseUrl = baseUrl.EndsWith("/") ? baseUrl : $"{baseUrl}/";
    }

    public static readonly string tituloHomePage = "PediTiscos";

    public static readonly string enderecoHome = "Quinta do Cardeal, Freixedo, Portugal";

    public static readonly string PerfilImagemPadrao = "Resources/Images/headicon.png";
}
