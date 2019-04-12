using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoGoMeter.Model;
using Team23.TelegramSkeleton;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PoGoMeter.Handlers
{
  [MessageEntityType(EntityType = MessageEntityType.BotCommand)]
  public class BotCommandMessageEntityHandler : IMessageEntityHandler
  {
    private readonly ITelegramBotClient myBot;
    private readonly PoGoMeterContext myContext;
    private readonly Pokemons myPokemons;

    public BotCommandMessageEntityHandler(ITelegramBotClient bot, PoGoMeterContext context, Pokemons pokemons)
    {
      myBot = bot;
      myContext = context;
      myPokemons = pokemons;
    }
    
    public async Task<bool?> Handle(MessageEntityEx data, object context = null, CancellationToken cancellationToken = default)
    {
      if (data.Value.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
      {
        await myBot.SendTextMessageAsync(data.Message.Chat, @"Enter target CP with command `cpXXXX`.", ParseMode.Markdown, cancellationToken: cancellationToken);
        return true;
      }

      var ignore = data.Value.StartsWith("/ignore", StringComparison.OrdinalIgnoreCase);
      var unignore = data.Value.StartsWith("/unignore", StringComparison.OrdinalIgnoreCase);
      if (ignore || unignore)
      {
        var ignores = await myContext.Ignore.Where(_ => _.UserId == data.Message.From.Id).ToDictionaryAsync(_ => _.Pokemon, cancellationToken);
        foreach (var segment in data.AfterValue.Split(new [] {','}).Select(_ => _.Trim()))
        {
          if (!short.TryParse(segment, out var number))
          {
            if (myPokemons.GetPokemonNumber(segment.ToString()) is short found)
            {
              number = found;
            }
            else
            {
              continue;
            }
          }
          var exists = ignores.ContainsKey(number);
          if (ignore && !exists)
          {
            var entity = new Ignore { UserId = data.Message.From.Id, Pokemon = number};
            ignores.Add(number, entity);
            myContext.Ignore.Add(entity);
          }
          else if (unignore && exists)
          {
            if (ignores.Remove(number, out var entity))
              myContext.Ignore.Remove(entity);
          }
        }

        await myContext.SaveChangesAsync(cancellationToken);
        
        await myBot.SendTextMessageAsync(data.Message.Chat,
          ignores.Values.Aggregate(new StringBuilder("Ignored pokemons: "), (builder, _) => builder.Append($"#{_.Pokemon} {myPokemons.GetPokemonName(_.Pokemon)},")).ToString(), ParseMode.Markdown, cancellationToken: cancellationToken);
        
        return true;
      }
      return null;
    }
  }
}