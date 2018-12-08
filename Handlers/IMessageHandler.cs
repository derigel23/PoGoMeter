using System;
using JetBrains.Annotations;
using Team23.TelegramSkeleton;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoGoMeter.Handlers
{
  public interface IMessageHandler : IMessageHandler<object, bool?> { }
  
  [MeansImplicitUse]
  public class MessageTypeAttribute : Attribute, IHandlerAttribute<Message, object>
  {
    public MessageType MessageType { get; set; }

    public bool ShouldProcess(Message message, object context)
    {
      return message.Type == MessageType;
    }
  }
}