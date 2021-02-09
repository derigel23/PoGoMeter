using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PoGoMeter
{
  public class Pokemons
  {
    private readonly Dictionary<short, string> myPokemonNames = new Dictionary<short, string>();
    private readonly Dictionary<string, short> myPokemonNumbers = new Dictionary<string, short>(StringComparer.OrdinalIgnoreCase);

    private static string pokemonNamePrefix = "pokemon_name_";
    
    public Pokemons()
    {
      foreach (var textResource in new[] { "GAME_TEXTS_1.txt", "GAME_TEXTS_2.txt" })
      {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(textResource);

        using var reader = new StreamReader(stream ?? throw new ArgumentNullException(), Encoding.UTF8);

        var key = default(string);
        while (reader.ReadLine() is { } line)
        {
          var lineParts = line.Split(new[] {':'}, 2);
          var value = lineParts.Skip(1).FirstOrDefault()?.Trim();
          switch (lineParts[0])
          {
            case "RESOURCE ID":
              key = value;
              break;
            case "TEXT" when !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value):
              if (key.StartsWith(pokemonNamePrefix) && short.TryParse(key.Substring(pokemonNamePrefix.Length), out var number))
              {
                if (!myPokemonNames.TryAdd(number, value))
                {
                  var existingValue = myPokemonNames[number];
                  if (existingValue != value)
                  {
                    Console.Error.WriteLineAsync($"Mismatching value for {number}: {value} -> {existingValue}");
                  }
                }
                else
                {
                  myPokemonNumbers.TryAdd(value, number);
                }
              }
              goto default;
            default:
              key = default;
              break;
          }
        }
      }
    }

    public string GetPokemonName(short number) => myPokemonNames.TryGetValue(number, out var name) ? name: null;
    public short? GetPokemonNumber(string name) => myPokemonNumbers.TryGetValue(name, out var number) ? number : default(short?);
  }
}