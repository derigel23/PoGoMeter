using System;
using System.Collections.Generic;
using Autofac.Features.Metadata;
using Microsoft.ApplicationInsights;
using PoGoMeter.Handlers;
using Team23.TelegramSkeleton;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PoGoMeter.Controllers
{
  public class TelegramController : Team23.TelegramSkeleton.TelegramController
  {
    public TelegramController(ITelegramBotClient botClient, TelemetryClient telemetryClient, IEnumerable<Meta<Func<Update, IUpdateHandler<bool?>>, UpdateHandlerAttribute>> updateHandlers)
      : base(telemetryClient, updateHandlers, botClient.GetType().Name) { }
  }
}
