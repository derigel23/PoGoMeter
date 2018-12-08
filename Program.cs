using System;
using System.Linq;

namespace PoGoMeter
{
  class Program
  {
    public static void Main(string[] args)
    {
      var calculator = new CalculatorCP();
      var lookup = calculator.Lookup;
      
      var input = args.FirstOrDefault();
      if (string.IsNullOrEmpty(input))
      {
        Console.Write("Input target CP:");
        input = Console.ReadLine();
      }
      if (int.TryParse(input, out var targetCP))
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
            Console.Write($" {(foundPokemon.attackIV + foundPokemon.defenseIV + foundPokemon.staminaIV) / (0m + calculator.MaxIV * 3) * 100,3:00}% {ShowIV(foundPokemon.attackIV)}/{ShowIV(foundPokemon.defenseIV)}/{ShowIV(foundPokemon.staminaIV)} Lvl {foundPokemon.lvl,-5}");
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
}
