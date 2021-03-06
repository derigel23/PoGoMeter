﻿using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Team23.TelegramSkeleton;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoGoMeter.Handlers
{
  public interface IMessageEntityHandler : IMessageEntityHandler<object, bool?> { }
  
  [MeansImplicitUse]
  [BaseTypeRequired(typeof(IMessageEntityHandler))]
  public class MessageEntityTypeAttribute : DescriptionAttribute, IHandlerAttribute<MessageEntity, object>
  {
    public MessageEntityType EntityType { get; set; }

    public bool ShouldProcess(MessageEntity messageEntity, object context)
    {
      return messageEntity.Type == EntityType;
    }

    public int Order => (int) EntityType;
  }
}