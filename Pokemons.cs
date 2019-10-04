using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace PoGoMeter
{
  public class Pokemons
  {
    private readonly Dictionary<short, string> myPokemonNames = new Dictionary<short, string>();
    private readonly Dictionary<string, short> myPokemonNumbers = new Dictionary<string, short>(StringComparer.OrdinalIgnoreCase);

    private static string pokemonNamePrefix = "pokemon_name_";
    
    public Pokemons()
    {
      using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GAME_MASTER.txt"))
      using (var reader = new StreamReader(stream, Encoding.UTF8))
      {
        while (reader.ReadLine()?.Trim() is string line) 
        {
          if (line == "Entry data")
          {
            var match = Regex.Match(reader.ReadLine() + reader.ReadLine(), "string Key = \"(?<key>.*)\"\\s+string Translation = \"(?<value>.*)\"", RegexOptions.Multiline);
            if (match.Success)
            {
              var key = match.Groups["key"].Value;
              var value = match.Groups["value"].Value;
              if (key.StartsWith(pokemonNamePrefix) && short.TryParse(key.Substring(pokemonNamePrefix.Length), out var number))
              {
                myPokemonNames.Add(number, value);
                myPokemonNumbers.Add(value, number);
              }
            }
          }
        }
      }
    }

    public string GetPokemonName(short number) => myPokemonNames.TryGetValue(number, out var name) ? name: null;
    public short? GetPokemonNumber(string name) => myPokemonNumbers.TryGetValue(name, out var number) ? number : default(short?);
  }
}