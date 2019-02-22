using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace PoGoMeter
{
  public class Pokemons
  {
    private readonly ConcurrentDictionary<string, string> myPokemons = new ConcurrentDictionary<string, string>();
    
    public Pokemons()
    {
      using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PoGoMeter.PoGoAssets.static_assets.txt.merged #6.txt"))
      using (var reader = new StreamReader(stream, Encoding.UTF8))
      {
        while (reader.ReadLine()?.Trim() is string line) 
        {
          if (line == "Entry data")
          {
            var match = Regex.Match(reader.ReadLine() + reader.ReadLine(), "string Key = \"(?<key>.*)\"\\s+string Translation = \"(?<value>.*)\"", RegexOptions.Multiline);
            if (match.Success)
            {
              myPokemons[match.Groups["key"].Value] = match.Groups["value"].Value;
            }
          }
        }
      }
    }

    public string GetPokemonName(int pokemon) => myPokemons.GetValueOrDefault($"pokemon_name_{pokemon:0000}");
  }
}