using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PoGoMeter
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost
        .CreateDefaultBuilder(args)
        .ConfigureServices(services => services.AddAutofac())
        .UseStartup<Startup>()
        .UseApplicationInsights()
        .ConfigureAppConfiguration((context, builder) =>
        {
          builder.AddJsonFile("appsettings.user.json", true);
          if (!string.IsNullOrEmpty(context.HostingEnvironment.EnvironmentName))
            builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.user.json", true);
        });
  }
}
