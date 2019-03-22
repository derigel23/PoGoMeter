using System;
using System.Collections.Generic;
using Autofac.Features.Metadata;
using Team23.TelegramSkeleton;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoGoMeter.Handlers
{
  [UpdateHandler(UpdateType = UpdateType.Message)]
  public class MessageUpdateHandler : MessageUpdateHandler<IMessageHandler, object, bool?, MessageTypeAttribute>
  {
    public MessageUpdateHandler(IEnumerable<Meta<Func<Message, IMessageHandler>, MessageTypeAttribute>> messageHandlers)
      : base(messageHandlers) { }
  }
}