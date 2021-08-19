using System;
using System.Collections.Generic;
using Team23.TelegramSkeleton;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PoGoMeter.Handlers
{
  public class StatusController : StatusController<object, bool?, BotCommandAttribute>
  {
    public StatusController(IEnumerable<ITelegramBotClient> bots, IEnumerable<IStatusProvider> statusProviders, IEnumerable<Lazy<Func<Message, IBotCommandHandler<object, bool?>>, BotCommandAttribute>> commandHandlers) : base(bots, statusProviders, commandHandlers)
    {
    }
  }
}