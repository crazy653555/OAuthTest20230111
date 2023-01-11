using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OAuth.Line.Core.LineLogin;

namespace OAuth.Line.Core;

public static class CoreServiceCollectionExtenstions
{
    public static void AddCoreLibs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();

        services.AddScoped<LineLoginService>();
        services.AddScoped<LineLoginConfig>();
    }
}