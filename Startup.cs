using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PoGoMeter.Configuration;
using PoGoMeter.Handlers;
using Telegram.Bot;

namespace PoGoMeter
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      // Adds services required for using options.
      services.AddOptions();

      // Register configuration handlers
      services.Configure<BotConfiguration>(Configuration.GetSection("BotConfiguration"));

      var culture = Configuration["Culture"];
      if (!string.IsNullOrEmpty(culture))
      {
        services.Configure<RequestLocalizationOptions>(options =>
        {
          options.DefaultRequestCulture = new RequestCulture(culture);
          options.RequestCultureProviders = null;
        });
      }

      services.AddMemoryCache();
      services.AddHttpClient();
      services.AddHttpClient<ITelegramBotClient, PoGoMeterTelegramBotClient>();

      services
        .AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Latest)
        .AddControllersAsServices();
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
      builder.RegisterModule<RegistrationModule>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      app
        .UseRequestLocalization()
        .UseStaticFiles()
        .UseMvc();
    }
  }
}
