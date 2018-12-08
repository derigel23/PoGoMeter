using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Metadata;
using Microsoft.ApplicationInsights;
using PoGoMeter.Handlers;
using Team23.TelegramSkeleton;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PoGoMeter.Controllers
{
  public class TelegramController : TelegramController<object, bool?, MessageTypeAttribute, object, CallbackQueryAttribute>
  {
    public TelegramController(TelemetryClient telemetryClient, ITelegramBotClient telegramBotClient, IEnumerable<Meta<Func<Message, IMessageHandler<object, bool?>>, MessageTypeAttribute>> messageHandlers, IEnumerable<Meta<Func<Update, ICallbackQueryHandler<object>>, CallbackQueryAttribute>> callbackQueryHandlers, IEnumerable<Meta<Func<Update, IInlineQueryHandler>, InlineQueryHandlerAttribute>> inlineQueryHandlers, IEnumerable<Func<Update, IChosenInlineResultHandler>> chosenInlineResultHandlers)
      : base(telemetryClient, telegramBotClient, messageHandlers, callbackQueryHandlers, inlineQueryHandlers, chosenInlineResultHandlers) { }

    protected override Task<bool?> ProcessMessage(Func<Message, object, IDictionary<string, string>, CancellationToken, Task<bool?>> processor, Message message, CancellationToken cancellationToken = new CancellationToken())
    {
      try
      {
        return base.ProcessMessage(processor, message, cancellationToken);
      }
      catch (Exception ex)
      {
        throw;
      }
    }
  }
}
