using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Team23.TelegramSkeleton;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoGoMeter.Handlers
{
  public interface IMessageEntityHandler : IMessageEntityHandler<object, bool?> { }
  
  [MeansImplicitUse]
  [BaseTypeRequired(typeof(IMessageEntityHandler))]
  public class MessageEntityTypeAttribute : Attribute, IHandlerAttribute<MessageEntity, object>
  {
    public MessageEntityType EntityType { get; set; }

    public bool ShouldProcess(MessageEntity messageEntity, object context)
    {
      return messageEntity.Type == EntityType;
    }

    public int Order => (int) EntityType;
  }
  
  public interface IBotCommandHandler : IMessageEntityHandler { }
  
  [MeansImplicitUse]
  [BaseTypeRequired(typeof(IBotCommandHandler))]
  public class BotCommandAttribute : MessageEntityTypeAttribute, IBotCommandHandlerAttribute<object>
  {
    public bool ShouldProcess(MessageEntityEx data, object context) => 
      BotCommandHandler.ShouldProcess(this, data, context);

    public BotCommandScope[] Scopes { get; set; }
    public BotCommand Command { get; set; }
    public string[] Aliases { get; set; }
  }
}