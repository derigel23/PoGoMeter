﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoGoMeter.Model;

namespace PoGoMeter.Migrations
{
  public class StatsFillingMigration
  {
    private readonly PoGoMeterContext myContext;
    private readonly string myConnectionString;
    
    private byte LevelMin { get; }
    private byte LevelMax { get; }

    private const byte MinIV = 0;
    private const byte MaxIV = 15;

    public StatsFillingMigration(PoGoMeterContext context, string connectionString, byte levelMin = 0, byte levelMax = MAX_LEVEL)
    {
      myContext = context;
      myConnectionString = connectionString;
      LevelMin = levelMin;
      LevelMax = levelMax;
    }
    
    private const short MEGA_OFFSET = 1000;
    
    public const byte MAX_LEVEL = 51 * 2 - 1;
    
    public async Task Run(bool clearData = false, CancellationToken cancellationToken = default)
    {
      await ClearEntities<Stats>(cancellationToken);
      await ClearEntities<BaseStats>(cancellationToken);
      await ClearEntities<PokemonName>(cancellationToken);

      var names = new Pokemons();
      using var bulkCopy = new SqlBulkCopy(myConnectionString, SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.UseInternalTransaction | SqlBulkCopyOptions.KeepIdentity) { EnableStreaming = true };
      double[] CPMs = null;
      var records = new List<Stats>();

      await using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GAME_MASTER.json");
      using var sr = new StreamReader(resourceStream);
      using var reader = new JsonTextReader(sr);
      var jsonSerializer = JsonSerializer.CreateDefault();
      while (await reader.ReadAsync(cancellationToken))
        if (reader.TokenType == JsonToken.StartObject)
        {
          if (reader.Path == "") continue;
          dynamic template = jsonSerializer.Deserialize(reader);
          string templateId = template["templateId"];
          if (templateId == "PLAYER_LEVEL_SETTINGS")
          {
            if (template["data"]["playerLevel"]["cpMultiplier"] is JArray levelCPMs)
            {
              CPMs = new Double[levelCPMs.Count * 2 - 1];
              for (var i = 0; i < levelCPMs.Count; i++)
              {
                var cpm = CPMs[i * 2] = (double) levelCPMs[i];
                if (i < levelCPMs.Count - 1)
                {
                  var nextCpm = (double) levelCPMs[i + 1];
                  CPMs[i * 2 + 1] = Math.Sqrt((cpm * cpm + nextCpm * nextCpm) / 2);
                }
              }
            }

            continue;
          }

          if (!(Regex.Match(templateId, @"^V(?<id>\d+)_.+") is var match && match.Success)) continue;
          if (CPMs == null) throw new InvalidProgramException("CPMs should be initialized first");
          var pokemon = short.Parse(match.Groups["id"].Value);
          if (pokemon > MEGA_OFFSET) throw new ArgumentOutOfRangeException("templateId", "Too many pokemons!");
          var name = names.GetPokemonName(pokemon);
          var data = template["data"];
          var settings = data[@"pokemonSettings"];
          if (settings == null) continue;
          if (settings.ContainsKey(@"form")) continue; // 'form' no custom form currently

          (short? baseAttack, short? baseDefense, short? baseStamina) GetBaseStats(dynamic root)
          {
            var stats = root[@"stats"];
            short? baseAttack = stats[@"baseAttack"];
            short? baseDefense = stats[@"baseDefense"];
            short? baseStamina = stats[@"baseStamina"];
            return (baseAttack, baseDefense, baseStamina);
          }

          if (!(GetBaseStats(settings) is (short atk, short def, short sta)))
          {
            await Console.Error.WriteLineAsync($"Unknown stats for {pokemon,4} {name}");
            continue;
          }

          var pokemonBaseStats = new List<(short, (short, short, short))>(1)
          {
            (pokemon, (atk, def, sta))
          };
          var pokemonNames = new List<PokemonName>(1)
          {
            new PokemonName
            {
              Pokemon = pokemon,
              Name = name
            }
          };
          if (settings[@"tempEvoOverrides"] is JArray megaForms)
          {
            for (var index = 0; index < megaForms.Count; index++)
            {
              var pokemonNumber = checked((short) ((index + 1) * MEGA_OFFSET + pokemon));
              dynamic megaForm = megaForms[index];
              string megaName;
              switch (megaForm["tempEvoId"]?.ToString())
              {
                case "TEMP_EVOLUTION_MEGA":
                  megaName = $"Mega {name}";
                  pokemonNames.Add(new PokemonName {Pokemon = pokemonNumber, Name = megaName});
                  break;
                case "TEMP_EVOLUTION_MEGA_X":
                  megaName = $"Mega {name} X";
                  pokemonNames.Add(new PokemonName {Pokemon = pokemonNumber, Name = megaName});
                  break;
                case "TEMP_EVOLUTION_MEGA_Y":
                  megaName = $"Mega {name} Y";
                  pokemonNames.Add(new PokemonName {Pokemon = pokemonNumber, Name = megaName});
                  break;
                default:
                  throw new ArgumentOutOfRangeException("megaForm", "Unknown pokemon mega form");
              }

              if (GetBaseStats(megaForm) is (short megaAtk, short megaDef, short megaSta))
              {
                pokemonBaseStats.Add((pokemonNumber, (megaAtk, megaDef, megaSta)));
              }
              else
              {
                await Console.Error.WriteLineAsync($"Unknown stats for {pokemonNumber,4} {megaName}");
              }
            }
          }

          await BulkCopy(bulkCopy, pokemonNames, cancellationToken);

          foreach (var (pokemonNumber, (baseAttack, baseDefense, baseStamina)) in pokemonBaseStats)
          {
            var pokemonName = pokemonNames.Single(_ => _.Pokemon == pokemonNumber).Name;
            await BulkCopy(bulkCopy, new[]
            {
              new BaseStats
              {
                Pokemon = pokemonNumber,
                Attack = baseAttack,
                Defense = baseDefense,
                Stamina = baseStamina
              }
            }, cancellationToken);

            for (var cpmIndex = Math.Max((byte) 0, LevelMin); cpmIndex < Math.Min(CPMs.Length, LevelMax); cpmIndex++)
            {
              var cpm = CPMs[cpmIndex];
              await Console.Out.WriteLineAsync(
                $"Pokemon {pokemonNumber,4} Level {cpmIndex / 2m + 1,-5} {pokemonName}");
              for (var attackIV = MinIV; attackIV <= MaxIV; attackIV++)
              for (var defenseIV = MinIV; defenseIV <= MaxIV; defenseIV++)
              for (var staminaIV = MinIV; staminaIV <= MaxIV; staminaIV++)
              {
                var attack = (short) (baseAttack + attackIV);
                var defense = (short) (baseDefense + defenseIV);
                var stamina = (short) (baseStamina + staminaIV);

                var cp = attack * Math.Sqrt(defense) * Math.Sqrt(stamina) * cpm * cpm / 10;
                var CP = Math.Max((short) 10, (short) Math.Floor(cp));
                records.Add(new Stats
                {
                  Pokemon = pokemonNumber,
                  AttackIV = attackIV,
                  DefenseIV = defenseIV,
                  StaminaIV = staminaIV,
                  Level = cpmIndex,
                  CP = CP
                });
              }
            }

            await BulkCopy(bulkCopy, records, cancellationToken);
            records.Clear();
          }
        }
    }

    private async Task ClearEntities<TEntity>(CancellationToken cancellationToken)
    {
      var entity = myContext.Model.FindEntityType(typeof(TEntity));

      var hasReferencingForeignKeys = entity.GetReferencingForeignKeys().Any();
      if (!hasReferencingForeignKeys)
      {
        await myContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {entity.GetSchema()}.{entity.GetTableName()}", cancellationToken);
      }
      else
      {
        await myContext.Database.ExecuteSqlRawAsync($"DELETE FROM {entity.GetSchema()}.{entity.GetTableName()}", cancellationToken);
      }
    }
    
    private async Task BulkCopy<TEntity>(SqlBulkCopy bulkCopy, IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
      var type = typeof(TEntity);
      var entity = myContext.Model.FindEntityType(type);

      if (StoreObjectIdentifier.Create(entity, StoreObjectType.Table) is not {} identifier)
        throw new ArgumentException("Can't get table name", nameof(TEntity));
      bulkCopy.DestinationTableName = $"{identifier.Schema}.{identifier.Name}";
      bulkCopy.ColumnMappings.Clear();
      foreach (var property in entity.GetProperties())
      {
        if (property.IsShadowProperty()) continue;
        bulkCopy.ColumnMappings
          .Add(property.Name, property.GetColumnName(identifier));
      }

      var members = new string[bulkCopy.ColumnMappings.Count];
      for (var i = 0; i < bulkCopy.ColumnMappings.Count; i++)
      {
        members[i] = bulkCopy.ColumnMappings[i].SourceColumn;
      }

      await using var objectReader = ObjectReader.Create(entities, members);
      await bulkCopy.WriteToServerAsync(objectReader, cancellationToken);
    }
  }
}