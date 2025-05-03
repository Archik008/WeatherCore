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
                services.AddDbContext<AppDbContext>();
                services.AddScoped<ICityRepository, CityRepository>();
                services.AddScoped<IDayWeatherRepository, DayWeatherRepository>();
                services.AddScoped<IHourWeatherRepository, HourWeatherRepository>();
                services.AddScoped<IWeatherService, WeatherService>();
                services.AddScoped<Form1>(); // зарегистрируй форму тоже
            });
}
