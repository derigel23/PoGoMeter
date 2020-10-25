using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PoGoMeter
{
  public class Pokemons
  {
    private readonly Dictionary<short, string> myPokemonNames = new Dictionary<short, string>();
    private readonly Dictionary<string, short> myPokemonNumbers = new Dictionary<string, short>(StringComparer.OrdinalIgnoreCase);

    private static string pokemonNamePrefix = "pokemon_name_";
    
    public Pokemons()
    {
      using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GAME_TEXTS.json");
      using var reader = new StreamReader(stream, Encoding.UTF8);
      using var jsonReader = new JsonTextReader(reader);
      
      if (JToken.ReadFrom(jsonReader)["data"] is JArray data)
      {
        var i = 0;
        var key = "";
        foreach (var element in data)
        {
          var value = element.ToString();
          if (i++ % 2 == 1)
          {
            if (key.StartsWith(pokemonNamePrefix) && short.TryParse(key.Substring(pokemonNamePrefix.Length), out var number))
            {
              myPokemonNames.Add(number, value);
              myPokemonNumbers.Add(value, number);
            }
          }
          else
          {
            key = value;
          }
        }
      }
    }

    public string GetPokemonName(short number) => myPokemonNames.TryGetValue(number, out var name) ? name: null;
    public short? GetPokemonNumber(string name) => myPokemonNumbers.TryGetValue(name, out var number) ? number : default(short?);
  }
}