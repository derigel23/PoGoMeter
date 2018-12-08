using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Metadata;
using Team23.TelegramSkeleton;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoGoMeter.Handlers
{
  [MessageType(MessageType = MessageType.Text)]
  public class TextMessageHandler : TextMessageHandler<object, bool?, MessageEntityTypeAttribute>
  {
    private readonly CalculatorCP myCalculator;
    private readonly ITelegramBotClient myBot;

    public TextMessageHandler(IEnumerable<Meta<Func<Message, IMessageEntityHandler<object, bool?>>, MessageEntityTypeAttribute>> messageEntityHandlers, CalculatorCP calculator, ITelegramBotClient bot)
      : base(messageEntityHandlers)
    {
      myCalculator = calculator;
      myBot = bot;
    }

    private const int SIZE_LIMIT = 4096;

    public override async Task<bool?> Handle(Message message, object context, CancellationToken cancellationToken = new CancellationToken())
    {

      if (string.IsNullOrEmpty(message.Text))
        return null;

      var result = await base.Handle(message, context, cancellationToken);

      if (result != null)
        return result;

      var lookup = myCalculator.Lookup;

      if (Regex.Match(message.Text, "cp(?<cp>\\d+)") is var cpMatch && !cpMatch.Success)
      {
        await myBot.SendTextMessageAsync(message.Chat, @"Unknown command. Enter target CP with command `cpXXXX`.", ParseMode.Markdown, cancellationToken: cancellationToken);
        return true;
      }

      var outputs = new Queue<string>();
      if (int.TryParse(cpMatch.Groups["cp"].Value, out var targetCP))
      {
        if (!lookup.Contains(targetCP))
        {
          await myBot.SendTextMessageAsync(message.Chat, "No such pokemon", ParseMode.Markdown, cancellationToken: cancellationToken);
          return true;
        }

        StringBuilder Finish(StringBuilder builder) => builder.AppendLine("```");

        var output = new StringBuilder()
          .AppendLine("```")
          .AppendLine($"Target CP {targetCP}")
          .AppendLine("Stats Attack/Defense/HP")
          .AppendLine();
        var prefixLength = output.Length;

        foreach (var foundPokemons in lookup[targetCP].ToLookup(data => data.name))
        {
          int before = output.Length;
          output.AppendLine($"{foundPokemons.Key}");
          foreach (var foundPokemon in foundPokemons
            .OrderByDescending(foundPokemon => foundPokemon.attackIV + foundPokemon.defenseIV + foundPokemon.staminaIV)
            .ThenByDescending(_ => _.lvl20CP))
          {
            output.Append($" {(foundPokemon.attackIV + foundPokemon.defenseIV + foundPokemon.staminaIV) / (0m + myCalculator.MaxIV * 3) * 100,3:00}% {ShowIV(foundPokemon.attackIV)}/{ShowIV(foundPokemon.defenseIV)}/{ShowIV(foundPokemon.staminaIV)} Lvl {foundPokemon.lvl,-5}");
            //              if (foundPokemon.lvl20CP is int lvl20CP)
            //              {
            //                output.Append($" (Lvl20 {lvl20CP}CP)");
            //              }
            output.AppendLine();
          }
          output.AppendLine();
          var after = output.Length;
          Finish(output);

          if (output.Length > SIZE_LIMIT)
          {
            outputs.Enqueue(output.ToString(0, before) + output.ToString(after, output.Length - after));
            output.Length = after;
            output.Remove(prefixLength, before - prefixLength);
          }
          else
          {
            output.Length = after;
          }
        }

        outputs.Enqueue(Finish(output).ToString());
      }

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

      foreach (var outputText in outputs)
      try
      {
        await myBot.SendTextMessageAsync(message.Chat, outputText, ParseMode.Markdown, cancellationToken: cancellationToken);
      }
      catch (Exception ex)
      {
        throw;
      }

      return true;
    }
  }
}