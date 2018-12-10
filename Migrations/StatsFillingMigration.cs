using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FastMember;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using PoGoMeter.Model;

namespace PoGoMeter.Migrations
{
  public class StatsFillingMigration
  {
    private readonly IModel myModel;
    private readonly string myConnectionString;
    
    private byte LevelMin { get; }
    private byte LevelMax { get; }

    private const byte MinIV = 0;
    private const byte MaxIV = 15;

    public StatsFillingMigration(PoGoMeterContext context, string connectionString, byte levelMin = 0, byte? levelMax = null)
    {
      myModel = context.Model;
      myConnectionString = connectionString;
      LevelMin = levelMin;
      LevelMax = levelMax.GetValueOrDefault((byte) CPM.Length);
    }
    
    public async Task Run(CancellationToken cancellationToken = default)
    {
      var type = typeof(Stats);
      var entity = myModel.FindEntityType(type);
      var entityName = entity.Relational();

      using (var bulkCopy = new SqlBulkCopy(myConnectionString))
      {
        bulkCopy.DestinationTableName = $"{entityName.Schema}.{entityName.TableName}";
        foreach (var property in entity.GetProperties())
        {
          bulkCopy.ColumnMappings.Add(property.Name, property.Relational().ColumnName);
        }

        var data = new List<Stats>();

        using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PoGoMeter.PoGoAssets.gamemaster.gamemaster.json"))
        using (var sr = new StreamReader(resourceStream))
        using (var reader = new JsonTextReader(sr))
        {
          var jsonSerializer = JsonSerializer.CreateDefault();
          while (reader.Read())
            if (reader.TokenType == JsonToken.StartObject)
            {
              if (reader.Path == "") continue;
              dynamic template = jsonSerializer.Deserialize(reader);
              if (!template.ContainsKey("pokemonSettings")) continue;
              string templateId = template["templateId"];
              if (!(Regex.Match(templateId, @"V(?<id>\d+)_.+") is var match && match.Success)) continue;
              var pokemon = short.Parse(match.Groups["id"].Value);
              //var name = texts[$"pokemon_name_{pokemon:0000}"];
              var settings = template["pokemonSettings"];
              if (settings.ContainsKey("form")) continue; // no alola currently
              var stats = settings["stats"];
              short baseAttack = stats["baseAttack"];
              short baseDefense = stats["baseDefense"];
              short baseStamina = stats["baseStamina"];
              for (var cpmIndex = Math.Max((byte)0, LevelMin); cpmIndex < Math.Min(CPM.Length, LevelMax); cpmIndex++)
              {
                Console.WriteLine($"Pokemon {pokemon,-3} Level {cpmIndex,-3}");
                for (var attackIV = MinIV; attackIV <= MaxIV; attackIV++)
                for (var defenseIV = MinIV; defenseIV <= MaxIV; defenseIV++)
                for (var staminaIV = MinIV; staminaIV <= MaxIV; staminaIV++)
                {
                  var cpm = CPM[cpmIndex];
                  var attack = (short) (baseAttack + attackIV);
                  var defense = (short) (baseDefense + defenseIV);
                  var stamina = (short) (baseStamina + staminaIV);
      
                  var cp = attack * Math.Sqrt(defense) * Math.Sqrt(stamina) * cpm * cpm / 10;
                  var CP = Math.Max((short)10, (short) Math.Floor(cp));
                  data.Add(new Stats
                  {
                    Pokemon = pokemon,
                    Attack = attack,
                    Defense = defense,
                    Stamina = stamina,
                    AttackIV = attackIV,
                    DefenseIV = defenseIV,
                    StaminaIV = staminaIV,
                    Level = cpmIndex,
                    CP = CP
                  });
                }
              }
              
              using (var objectReader = ObjectReader.Create(data))
              {
                await bulkCopy.WriteToServerAsync(objectReader, cancellationToken);
              }
              data.Clear();
            }
          }
        }
      }

    private static double[] CPM =
    {
      0.094,
      0.135137432,
      0.16639787,
      0.192650919,
      0.21573247,
      0.236572661,
      0.25572005,
      0.273530381,
      0.29024988,
      0.306057377,
      0.3210876,
      0.335445036,
      0.34921268,
      0.362457751,
      0.37523559,
      0.387592406,
      0.39956728,
      0.411193551,
      0.42250001,
      0.432926419,
      0.44310755,
      0.4530599578,
      0.46279839,
      0.472336083,
      0.48168495,
      0.4908558,
      0.49985844,
      0.508701765,
      0.51739395,
      0.525942511,
      0.53435433,
      0.542635767,
      0.55079269,
      0.558830576,
      0.56675452,
      0.574569153,
      0.58227891,
      0.589887917,
      0.59740001,
      0.604818814,
      0.61215729,
      0.619399365,
      0.62656713,
      0.633644533,
      0.64065295,
      0.647576426,
      0.65443563,
      0.661214806,
      0.667934,
      0.674577537,
      0.68116492,
      0.687680648,
      0.69414365,
      0.700538673,
      0.70688421,
      0.713164996,
      0.71939909,
      0.725571552,
      0.7317,
      0.734741009,
      0.73776948,
      0.740785574,
      0.74378943,
      0.746781211,
      0.74976104,
      0.752729087,
      0.75568551,
      0.758630378,
      0.76156384,
      0.764486065,
      0.76739717,
      0.770297266,
      0.7731865,
      0.776064962,
      0.77893275,
      0.781790055,
      0.78463697,
      0.787473578,
      0.79030001,
    };
  }
}