using System;
using System.Threading;
using System.Threading.Tasks;
using Team23.TelegramSkeleton;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PoGoMeter.Handlers
{
  [MessageEntityType(EntityType = MessageEntityType.BotCommand)]
  public class BotCommandMessageEntityHandler : IMessageEntityHandler
  {
    private readonly ITelegramBotClient myBot;

    public BotCommandMessageEntityHandler(ITelegramBotClient bot)
    {
      myBot = bot;
    }
    
    public async Task<bool?> Handle(MessageEntityEx data, object context = null, CancellationToken cancellationToken = default)
    {
      if (data.Value.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
      {
        await myBot.SendTextMessageAsync(data.Message.Chat, @"Enter target CP with command `cpXXXX`.", ParseMode.Markdown, cancellationToken: cancellationToken);
        return true;
      }

      return null;
    }
  }
}