using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Team23.TelegramSkeleton;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoGoMeter.Handlers
{
  public interface IMessageHandler : IMessageHandler<object, bool?> { }
  
  [MeansImplicitUse]
  [BaseTypeRequired(typeof(IMessageHandler))]
  public class MessageTypeAttribute : Attribute, IHandlerAttribute<Message, (UpdateType, object)>
  {
    // by default only for new messages
    public MessageTypeAttribute() : this(UpdateType.Message, UpdateType.ChannelPost) { }

    public MessageTypeAttribute(params UpdateType[] updateTypes)
    {
      UpdateTypes = new HashSet<UpdateType>(updateTypes);
    }
    
    public MessageType MessageType { get; set; }

    public ISet<UpdateType> UpdateTypes { get; }
    
    public bool ShouldProcess(Message message, (UpdateType, object) context)
    {
      var (updateType, _) = context;
      return UpdateTypes.Contains(updateType) && message.Type == MessageType;
    }

    public int Order => (int) MessageType;
  }
}