using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;

namespace PoGoMeter
{
  class Program
  {
    const int maxIV = 15;
    const int minIV = 10;

    static void Main(string[] args)
    {
      var allCPs = new List<(int pokemon, string name, int attackIV, int defenseIV, int staminaIV, decimal lvl, int CP, int? lvl20CP)>();
      using (var resourceStream = typeof(Program).Assembly.GetManifestResourceStream("PoGoMeter.pokemon.json"))
      {
        var json = JsonObject.Load(resourceStream);
        foreach (JsonValue pokemonInfo in json)
        {
          int pokemon = pokemonInfo["dex"];
          string  name = pokemonInfo["name"];
          var stats = pokemonInfo["stats"];
          int baseAttack = stats["baseAttack"];
          int baseDefense = stats["baseDefense"];
          int baseStamina = stats["baseStamina"];
          int? lvl20cp = null;
          for (int attackIV = maxIV; attackIV >= minIV; attackIV--)
          for (int defenseIV = maxIV; defenseIV >= minIV; defenseIV--)
          for (int staminaIV = maxIV; staminaIV >= minIV; staminaIV--)
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

        var lookup = allCPs.ToLookup(data => data.CP);
        var input = args.FirstOrDefault();
        if (string.IsNullOrEmpty(input))
        {
          Console.Write("Input target CP:");
          input = Console.ReadLine();
        }
        if (int.TryParse(input, out int targetCP))
        {
          if (!lookup.Contains(targetCP))
            Console.WriteLine("No such pokemon");

          Console.WriteLine($"Target CP {targetCP}");
          Console.WriteLine("Stats Attack/Defense/HP");
          Console.WriteLine();
          foreach (var foundPokemons in lookup[targetCP].ToLookup(data => data.name))
          {
            Console.WriteLine($"{foundPokemons.Key}");
            foreach (var foundPokemon in foundPokemons
              .OrderByDescending(foundPokemon => foundPokemon.attackIV + foundPokemon.defenseIV + foundPokemon.staminaIV)
              .ThenByDescending(_ => _.lvl20CP))
            {
              Console.Write($" {(foundPokemon.attackIV + foundPokemon.defenseIV + foundPokemon.staminaIV) / (0m + maxIV + maxIV + maxIV) * 100,3:00}% {ShowIV(foundPokemon.attackIV)}/{ShowIV(foundPokemon.defenseIV)}/{ShowIV(foundPokemon.staminaIV)} Lvl {foundPokemon.lvl,-5}");
//              if (foundPokemon.lvl20CP is int lvl20CP)
//              {
//                Console.Write($" (Lvl20 {lvl20CP}CP)");
//              }
              Console.WriteLine();
            }
            Console.WriteLine();
          }
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
      }
    }
    
    static double[] CPM =
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
