using System;
using JetBrains.Annotations;
using Team23.TelegramSkeleton;
using Telegram.Bot.Types;

namespace PoGoMeter.Handlers
{
  [MeansImplicitUse]
  public class CallbackQueryAttribute : Attribute, IHandlerAttribute<CallbackQuery, object>
  {
    public bool ShouldProcess(CallbackQuery callbackQuery, object context) => true;
  }

}