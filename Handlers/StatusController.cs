using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PoGoMeter.Migrations;
using PoGoMeter.Model;
using Team23.TelegramSkeleton;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PoGoMeter.Handlers
{
  public class StatusController : StatusController<object, bool?, BotCommandAttribute>
  {
    private readonly PoGoMeterContext _db;

    public StatusController(IEnumerable<ITelegramBotClient> bots, IEnumerable<IStatusProvider> statusProviders, IEnumerable<Lazy<Func<Message, IBotCommandHandler<object, bool?>>, BotCommandAttribute>> commandHandlers, PoGoMeterContext db)
      : base(bots, statusProviders, commandHandlers)
    {
      _db = db;
    }
    
    [HttpGet("/schitalochka")]
    public async Task<IActionResult> Schitalochka(CancellationToken cancellationToken)
    {
      var names = new Pokemons();
      await using var buffer = new StringWriter();
      using (var writer = new MyJsonTextWriter(buffer))
      {
        await writer.WriteStartArrayAsync(cancellationToken);
        foreach (var pokemon in _db.BaseStats.Include(stats => stats.Name)
                   .OrderBy(stats => stats.Pokemon % StatsFillingMigration.MEGA_OFFSET).ToList())
        {
          var pokemonName = names.GetPokemonName((short)(pokemon.Pokemon % StatsFillingMigration.MEGA_OFFSET));
          var name = (pokemonName + pokemon.Name.Name.Replace(pokemonName, ""))
            .Replace(" ", "")
            .Replace("\u2640", "Female")
            .Replace("\u2642", "Male");
          await writer.WriteStartArrayAsync(cancellationToken);
          await writer.WriteValueAsync(name, cancellationToken);
          await writer.WriteValueAsync(pokemon.Stamina, cancellationToken);
          await writer.WriteValueAsync(pokemon.Attack, cancellationToken);
          await writer.WriteValueAsync(pokemon.Defense, cancellationToken);
          await writer.WriteEndArrayAsync(cancellationToken);
        }
        await writer.WriteEndArrayAsync(cancellationToken);
      }
      return Content(buffer.ToString(), "application/json");
    }

    private class MyJsonTextWriter : JsonTextWriter
    {
      public MyJsonTextWriter(TextWriter buffer) : base(buffer) { }

      
      protected override void WriteValueDelimiter()
      {
        base.WriteValueDelimiter();
        if (WriteState == WriteState.Array && Top == 1)
          base.WriteRaw(Environment.NewLine);
      }
    }
  }
}