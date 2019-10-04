using System.Threading;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PoGoMeter.Migrations;
using PoGoMeter.Model;

namespace PoGoMeter
{
  public static class Program
  {
    public static async Task Main(string[] args)
    {
      var webHost = CreateWebHostBuilder(args).Build();
      var cancellationToken = CancellationToken.None;
      using (var serviceScope = webHost.Services.CreateScope())
      {
        var configuration = serviceScope.ServiceProvider.GetService<IConfiguration>();
        if (configuration.GetValue("InitData", false))
        {
          var fillingMigration = new StatsFillingMigration(serviceScope.ServiceProvider.GetService<PoGoMeterContext>(),
            configuration.GetConnectionString("PoGoMeterDatabase"));
          
          await fillingMigration.Run(cancellationToken);
          return;
        }
      }
      
      await webHost.RunAsync(cancellationToken);
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost
        .CreateDefaultBuilder(args)
        .ConfigureServices(services => services.AddAutofac())
        .UseStartup<Startup>()
        .ConfigureAppConfiguration((context, builder) =>
        {
          builder.AddJsonFile("appsettings.user.json", true);
          if (!string.IsNullOrEmpty(context.HostingEnvironment.EnvironmentName))
            builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.user.json", true);
        });
  }
}
