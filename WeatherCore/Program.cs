using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherCore;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        var host = CreateHostBuilder().Build();

        ApplicationConfiguration.Initialize();
        var form = host.Services.GetRequiredService<Form1>();
        Application.Run(form);
    }

    static IHostBuilder CreateHostBuilder() =>
    Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddDbContextFactory<AppDbContext>();
            services.AddScoped<WeatherService>();
            services.AddScoped<Form1>();
            services.AddScoped<WeatherAPIManager>();
        });
}