using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Metadata;
using PoGoMeter.Model;
using Team23.TelegramSkeleton;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoGoMeter.Handlers
{
  [MessageType(MessageType = MessageType.Text)]
  public class TextMessageHandler : TextMessageHandler<object, bool?, MessageEntityTypeAttribute>, IMessageHandler
  {
    private readonly Pokemons myPokemons;
    private readonly ITelegramBotClient myBot;
    private readonly PoGoMeterContext myDb;

    private byte myMinIV = 10;
    
    public TextMessageHandler(IEnumerable<Meta<Func<Message, IMessageEntityHandler<object, bool?>>, MessageEntityTypeAttribute>> messageEntityHandlers,
      Pokemons pokemons, ITelegramBotClient bot, PoGoMeterContext db) : base(messageEntityHandlers)
    {
      myPokemons = pokemons;
      myBot = bot;
      myDb = db;
    }

    private const int SIZE_LIMIT = 4096;

    public override async Task<bool?> Handle(Message message, (UpdateType, object) __, CancellationToken cancellationToken = new CancellationToken())
    {

      if (string.IsNullOrEmpty(message.Text))
        return null;

      var result = await base.Handle(message, __, cancellationToken);

      if (result != null)
        return result;

      // https://pokemongo.gamepress.gg/guide-search-bar

      var query = myDb.Stats
        .Where(_ => _.AttackIV >= myMinIV)
        .Where(_ => _.DefenseIV >= myMinIV)
        .Where(_ => _.StaminaIV >= myMinIV);

      try
      {


      var queryOr = Enumerable.Empty<Stats>().AsQueryable(); ;
      foreach (var queryOrPart in message.Text.Split(new [] { "OR", ",", ";", ":" },StringSplitOptions.RemoveEmptyEntries))
      {
        var queryAnd = query;
        foreach (var queryAndPart in queryOrPart.Split(new [] { "AND", "&", "|" },StringSplitOptions.RemoveEmptyEntries))
        {
          if (int.TryParse(queryAndPart, out var pokemonNumber))
          {
            queryAnd = queryAnd.Where(_ => _.Pokemon == pokemonNumber);
          }
          else if (Regex.Match(queryAndPart, "^cp(?<cp>\\d+)$", RegexOptions.IgnoreCase) is var cpMatch && cpMatch.Success)
          {
            if (!int.TryParse(cpMatch.Groups["cp"].Value, out var targetCP))
              return false; // can't be

            queryAnd = queryAnd.Where(_ => _.CP == targetCP);
          }
          else
          {
            await myBot.SendTextMessageAsync(message.Chat, @"Unknown command. Enter target CP with command `cpXXXX`.", ParseMode.Markdown, cancellationToken: cancellationToken);
            return true;
          }
        }

        queryOr = queryOr.Concat(queryAnd);
      }

      await myBot.SendChatActionAsync(message.Chat, ChatAction.Typing, cancellationToken);

      var outputs = new List<string>();

      
      var stats = queryOr.ToList();

      if (stats.Count == 0)
      {
        await myBot.SendTextMessageAsync(message.Chat, "No such pokemon", ParseMode.Markdown, cancellationToken: cancellationToken);
        return true;
      }

      StringBuilder Finish(StringBuilder builder) => builder.AppendLine("```");

      var output = new StringBuilder()
        .AppendLine("```")
        //.AppendLine($"Target CP {targetCP}")
        .AppendLine("Stats Attack/Defense/HP")
        .AppendLine();
      var prefixLength = output.Length;

      foreach (var foundPokemons in stats.ToLookup(data => data.Pokemon).OrderBy(_ => _.Key))
      {
        int before = output.Length;
        output.AppendLine($"{myPokemons.GetPokemonName(foundPokemons.Key) ?? foundPokemons.Key.ToString()}");
        foreach (var foundPokemon in foundPokemons
          .OrderByDescending(foundPokemon => foundPokemon.AttackIV + foundPokemon.DefenseIV + foundPokemon.StaminaIV))
        {
          output.Append(
            $" {(foundPokemon.AttackIV + foundPokemon.DefenseIV + foundPokemon.StaminaIV) / (0m + 15 * 3) * 100,3:00}% {ShowIV(foundPokemon.AttackIV)}/{ShowIV(foundPokemon.DefenseIV)}/{ShowIV(foundPokemon.StaminaIV)} Lvl {foundPokemon.Level / 2m + 1,-5}");
          output.AppendLine();
        }

        output.AppendLine();
        var after = output.Length;
        Finish(output);

        if (output.Length > SIZE_LIMIT)
        {
          outputs.Add(output.ToString(0, before) + output.ToString(after, output.Length - after));
          output.Length = after;
          output.Remove(prefixLength, before - prefixLength);
        }
        else
        {
          output.Length = after;
        }
      }

      outputs.Add(Finish(output).ToString());

      string ShowIV(int iv)
      {
        switch (iv)
        {
          //case 0:
          //  return "\u24ea";
          //case int n when n > 0 && n <= maxIV:
          //  return ((char)('\u2460' + iv - 1)).ToString();
          default:
            return iv.ToString();
        }
      }

      foreach (var text in outputs)
        await myBot.SendTextMessageAsync(message.Chat, text, ParseMode.Markdown, cancellationToken: cancellationToken);
      }
      catch (Exception ex)
      {
      }

      return true;
    }
  }
}