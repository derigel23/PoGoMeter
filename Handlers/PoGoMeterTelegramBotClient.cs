using System.Net.Http;
using Microsoft.Extensions.Options;
using PoGoMeter.Configuration;
using Telegram.Bot;

namespace PoGoMeter.Handlers
{
  public class PoGoMeterTelegramBotClient : TelegramBotClient
  {
    public PoGoMeterTelegramBotClient(IOptions<BotConfiguration> options, HttpClient httpClient)
      : base(options.Value.BotToken, httpClient)
    {
      Timeout = options.Value.Timeout;
    }
  }
}