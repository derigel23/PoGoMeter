using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace PoGoMeter
{
  public class CalculatorCP
  {
    public int MinIV { get; }
    public int MaxIV { get; }

    public CalculatorCP(int minIV = 10, int maxIV = 15)
    {
      MinIV = minIV;
      MaxIV = maxIV;
      var texts = new Dictionary<string, string>();
      using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PoGoMeter.PoGoAssets.static_assets.txt.merged #20.txt"))
      using (var reader = new StreamReader(stream, Encoding.UTF8))
      {
        while (reader.ReadLine()?.Trim() is string line) 
        {
          if (line == "Entry data")
          {
            var match = Regex.Match(reader.ReadLine() + reader.ReadLine(), "string Key = \"(?<key>.*)\"\\s+string Translation = \"(?<value>.*)\"", RegexOptions.Multiline);
            if (match.Success)
            {
              texts[match.Groups["key"].Value] = match.Groups["value"].Value;
            }
          }
        }
      }

      var allCPs = new List<(int pokemon, string name, int attackIV, int defenseIV, int staminaIV, decimal lvl, int CP, int? lvl20CP)>();
      using (var resourceStream = typeof(Program).Assembly.GetManifestResourceStream("PoGoMeter.PoGoAssets.gamemaster.gamemaster.json"))
      {
        var json = JsonObject.Load(resourceStream);
        foreach (dynamic template in json["itemTemplates"])
        {
          if (!template.ContainsKey("pokemonSettings")) continue;
          string templateId = template["templateId"];
          if (!(Regex.Match(templateId, @"V(?<id>\d+)_.+") is var match && match.Success)) continue;
          var pokemon = int.Parse(match.Groups["id"].Value);
          var name = texts[$"pokemon_name_{pokemon:0000}"];
          var settings = template["pokemonSettings"];
          if (settings.ContainsKey("form")) continue; // no alola currently
          var stats = settings["stats"];
          int baseAttack = stats["baseAttack"];
          int baseDefense = stats["baseDefense"];
          int baseStamina = stats["baseStamina"];
          int? lvl20cp = null;
          for (int attackIV = MaxIV; attackIV >= MinIV; attackIV--)
          for (int defenseIV = MaxIV; defenseIV >= MinIV; defenseIV--)
          for (int staminaIV = MaxIV; staminaIV >= MinIV; staminaIV--)
          for (var cpmIndex = 0; cpmIndex < CPM.Length; cpmIndex++)
          {
            var cpm = CPM[cpmIndex];
            var attack = (baseAttack + attackIV);
            var defense = (baseDefense + defenseIV);
            var stamina = (baseStamina + staminaIV);

            var cp = attack * Math.Sqrt(defense) * Math.Sqrt(stamina) * cpm * cpm / 10;
            int CP = Math.Max(10, (int)Math.Floor(cp));
            allCPs.Add((pokemon, name, attackIV, defenseIV, staminaIV, cpmIndex / 2m + 1, CP, lvl20cp));
            if (cpmIndex == 38) // LVL20
            {
              lvl20cp = CP;
            }
          }
        }

        Lookup = allCPs.ToLookup(data => data.CP);
      }
    }

    public ILookup<int, (int pokemon, string name, int attackIV, int defenseIV, int staminaIV, decimal lvl, int CP, int? lvl20CP)> Lookup { get; }

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