using System.Net.Http;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using PoGoMeter.Configuration;
using Team23.TelegramSkeleton;

namespace PoGoMeter.Handlers
{
  public class PoGoMeterTelegramBotClient : TelegramBotClientEx
  {
    public PoGoMeterTelegramBotClient(TelemetryClient telemetryClient, IOptions<BotConfiguration> options, string token, HttpClient httpClient)
      : base(telemetryClient, token, httpClient)
    {
      Timeout = options.Value.Timeout;
    }
  }
}