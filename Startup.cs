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
using PoGoMeter.Model;
using Team23.TelegramSkeleton;
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
      services.AddApplicationInsightsTelemetry();
      
      // Adds services required for using options.
      services.AddOptions();

      // Register configuration handlers
      services.Configure<BotConfiguration>(Configuration.GetSection("BotConfiguration"));
      services.Configure<Settings>(Configuration.GetSection("Settings"));

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
        .AddMvc(options => options.EnableEndpointRouting = false)
        .AddNewtonsoftJson()
        .SetCompatibilityVersion(CompatibilityVersion.Latest)
        .AddApplicationPart(typeof(TelegramController).Assembly);

      services.AddDbContext<PoGoMeterContext>(options => options
        .UseSqlServer(Configuration.GetConnectionString("PoGoMeterDatabase"))
        .EnableSensitiveDataLogging());
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
      builder.RegisterModule<RegistrationModule>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
      app
        .UseRequestLocalization()
        .UseStaticFiles()
        .UseMvc();
    }
  }
}
