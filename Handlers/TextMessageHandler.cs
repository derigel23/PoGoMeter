﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PoGoMeter.Configuration;
using PoGoMeter.Migrations;
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
    private readonly TelemetryClient myTelemetryClient;
    private readonly ITelegramBotClient myBot;
    private readonly PoGoMeterContext myDb;

    private readonly byte myMinIV;
    private readonly byte myBestBuddyMinIV;
    private readonly byte myBestBuddyMinLevel;

    public TextMessageHandler(TelemetryClient telemetryClient, IOptions<Settings> settings, IEnumerable<Lazy<Func<Message, IMessageEntityHandler<object, bool?>>, MessageEntityTypeAttribute>> messageEntityHandlers, ITelegramBotClient bot, PoGoMeterContext db)
      : base(bot, messageEntityHandlers)
    {
      myTelemetryClient = telemetryClient;
      myBot = bot;
      myDb = db;
      myMinIV = settings.Value?.MinIV ?? 0;
      myBestBuddyMinIV = settings.Value?.BestBuddyMinIV ?? 0;
      myBestBuddyMinLevel = settings.Value?.BestBuddyMinLevel ?? 0;
    }

    private const int SIZE_LIMIT = 4096;

    public const byte MAX_LEVEL = StatsFillingMigration.MAX_LEVEL;
    
    public override async Task<bool?> Handle(Message message, (UpdateType, object) __, CancellationToken cancellationToken = new CancellationToken())
    {

      if (string.IsNullOrEmpty(message.Text))
        return null;

      var result = await base.Handle(message, __, cancellationToken);

      if (result != null)
        return result;

      // https://pokemongo.gamepress.gg/guide-search-bar

      IQueryable<Stats> rawQuery = myDb.Stats.OrderBy(stats => stats.Pokemon);
      
      IQueryable<Stats> filteredQuery = rawQuery
        // special criteria for Min IV
        .Where(_ => _.AttackIV >= myMinIV)
        .Where(_ => _.DefenseIV >= myMinIV)
        .Where(_ => _.StaminaIV >= myMinIV)
        // special criteria for best buddy
        .Where(_ => _.Level < myBestBuddyMinLevel || _.AttackIV >= myBestBuddyMinIV)
        .Where(_ => _.Level < myBestBuddyMinLevel || _.DefenseIV >= myBestBuddyMinIV)
        .Where(_ => _.Level < myBestBuddyMinLevel || _.StaminaIV >= myBestBuddyMinIV);
      
      try
      {
      IQueryable<Stats> queryOr = rawQuery.Take(0);
      
      foreach (var queryOrPart in message.Text.Split(new [] { "OR", ",", ";", ":" },StringSplitOptions.RemoveEmptyEntries))
      {
        bool levelFiltered = false;        
        bool noFilter = false;        
        var queryAndFilters = new List<Expression<Func<Stats, bool>>>() ;
        foreach (var queryAndPart in queryOrPart.Split(new [] { "AND", "&", "|" },StringSplitOptions.RemoveEmptyEntries))
        {
          if (short.TryParse(queryAndPart, NumberStyles.Any, CultureInfo.InvariantCulture, out var pokemonNumber))
          {
            queryAndFilters.Add(_ => _.Pokemon == pokemonNumber);
          }
          else if (Regex.Match(queryAndPart, "^cp(?<cp>\\d+)$", RegexOptions.IgnoreCase) is var cpMatch && cpMatch.Success)
          {
            if (!short.TryParse(cpMatch.Groups["cp"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var targetCP))
              return false; // can't be

            queryAndFilters.Add(_ => _.CP == targetCP);
          }
          else if (Regex.Match(queryAndPart, "^atk(?<atk>\\d+)$", RegexOptions.IgnoreCase) is var attackMatch && attackMatch.Success)
          {
            if (!byte.TryParse(attackMatch.Groups["atk"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var targetAttack))
              return false; // can't be

            queryAndFilters.Add(_ => _.AttackIV == targetAttack);
          }
          else if (Regex.Match(queryAndPart, "^def(?<def>\\d+)$", RegexOptions.IgnoreCase) is var defenseMatch && defenseMatch.Success)
          {
            if (!byte.TryParse(defenseMatch.Groups["def"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var targetDefense))
              return false; // can't be

            queryAndFilters.Add(_ => _.DefenseIV == targetDefense);
          }
          else if (Regex.Match(queryAndPart, "^sta(?<sta>\\d+)$", RegexOptions.IgnoreCase) is var staminaMatch && staminaMatch.Success)
          {
            if (!byte.TryParse(staminaMatch.Groups["sta"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var targetStamina))
              return false; // can't be

            queryAndFilters.Add(_ => _.StaminaIV == targetStamina);
          }
          else if (Regex.Match(queryAndPart, "^lvl(?<lvl>\\d+([\\.,]\\d+)?)$", RegexOptions.IgnoreCase) is var levelMatch && levelMatch.Success)
          {
            if (!decimal.TryParse(levelMatch.Groups["lvl"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var targetLevelDecimal))
              return false; // can't be

            levelFiltered = true;
            var targetLevel = (byte)((targetLevelDecimal - 1) * 2);
            queryAndFilters.Add(_ => _.Level == targetLevel);
          }
          else if (Regex.Match(queryAndPart, "^nofilter$", RegexOptions.IgnoreCase) is var noFilterMatch && noFilterMatch.Success)
          {
            noFilter = true;
          }
          else
          {
            // try find pokemon name
            var pokemonsByName = await myDb.Set<PokemonName>().Where(_ => EF.Functions.Like(_.Name, $"%{queryAndPart}%")).ToListAsync(cancellationToken);
            if (pokemonsByName.Count > 0)
            {
              var pokemonNumbers = pokemonsByName.Select(name => name.Pokemon).ToList();
              queryAndFilters.Add(_ => pokemonNumbers.Contains(_.Pokemon));
            }
            else
            {
              await myBot.SendTextMessageAsync(message.Chat, @"Unknown command. Enter target CP with command `cpXXXX`.", ParseMode.Markdown, cancellationToken: cancellationToken);
              return true;
            }
          }
        }

        if (!noFilter && !levelFiltered)
        {
          queryAndFilters.Add(_ => _.Level <= MAX_LEVEL);
        }
        
        queryOr = queryOr.Concat(queryAndFilters.Aggregate(noFilter ? rawQuery : filteredQuery, 
          (query, filter) => query.Where(filter)));
      }

      var ignore = await myDb.Ignore.Where(_ => _.UserId == message.From.Id).Select(_ => _.Pokemon).ToListAsync(cancellationToken);
      if (ignore.Count > 0)
      {
        queryOr = queryOr.Where(_ => !ignore.Contains(_.Pokemon));
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
        .AppendLine("```");

      var singleTargetCP = false;
      if (stats.ToLookup(_ => _.CP) is var targetCPs && (singleTargetCP = targetCPs.Count == 1))
      {
        output.AppendLine($"Target CP {targetCPs.Single().Key}");
      }
  
      output
        .AppendLine("Stats Attack/Defense/HP")
        .AppendLine();

      var prefixLength = output.Length;

      var foundPokemonsSet = stats.ToLookup(data => data.Pokemon);
      var foundPokemonNumbers = foundPokemonsSet.Select(_ => _.Key).ToList();
      var pokemonNames = await myDb.Set<PokemonName>().Where(name => foundPokemonNumbers.Contains(name.Pokemon)).ToDictionaryAsync(name => name.Pokemon, name => name.Name, cancellationToken);
      foreach (var foundPokemons in foundPokemonsSet.OrderBy(_ => _.Key))
      {
        void Header() => output.AppendLine($"{pokemonNames.GetValueOrDefault(foundPokemons.Key, foundPokemons.Key.ToString())}");
        Header();
        foreach (var foundPokemon in foundPokemons
          .OrderByDescending(foundPokemon => foundPokemon.AttackIV + foundPokemon.DefenseIV + foundPokemon.StaminaIV))
        {
          var before = output.Length;

          if (!singleTargetCP)
            output.Append($"{foundPokemon.CP,6}CP");
          output.AppendLine($" {(foundPokemon.AttackIV + foundPokemon.DefenseIV + foundPokemon.StaminaIV) / (0m + 15 * 3) * 100,3:00}% {ShowIV(foundPokemon.AttackIV)}/{ShowIV(foundPokemon.DefenseIV)}/{ShowIV(foundPokemon.StaminaIV)} Lvl {foundPokemon.Level / 2m + 1,-5}");
          
          var after = output.Length;
          Finish(output);

          if (output.Length > SIZE_LIMIT)
          {
            outputs.Add(output.ToString(0, before) + output.ToString(after, output.Length - after));
            output.Length = after;
            output.Remove(prefixLength, before - prefixLength);
            Header();
          }
          else
          {
            output.Length = after;
          }
        }
        output.AppendLine();
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
            return $"{iv,2}";
        }
      }

      foreach (var text in outputs)
      {
        await myBot.SendTextMessageAsync(message.Chat, text, ParseMode.Markdown, cancellationToken: cancellationToken);
        await Task.Delay(1000, cancellationToken);
      }
      }
      catch (Exception ex)
      {
        myTelemetryClient.TrackException(ex);
      }

      return true;
    }
  }
}