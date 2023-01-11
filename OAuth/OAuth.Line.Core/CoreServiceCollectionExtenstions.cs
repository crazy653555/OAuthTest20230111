using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OAuth.Line.Core.LineLogin;

namespace OAuth.Line.Core;

public static class CoreServiceCollectionExtenstions
{
    /// <summary>
    /// 將 Line Login 和 Line Notify 及相關聯動的核心程式加入 DI Container 中
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>

    public static void AddCoreLibs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();

        //設定Mapper資料，Mapper到OAth.Web中的appsettings.json
        services.Configure<LineLoginConfig>(configuration.GetSection("LineLogin"));

        //設定class
        services.AddScoped<LineLoginService>();
    }
}