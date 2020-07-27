using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

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
      using var sr = new StreamReader(stream);
      using var reader = new JsonTextReader(sr);
      while (reader.Read())
      {
        if (reader.TokenType == JsonToken.String)
        {
          if (reader.Value is string key && key.StartsWith(pokemonNamePrefix) && short.TryParse(key.Substring(pokemonNamePrefix.Length), out var number))
          {
            var value = reader.ReadAsString();

            myPokemonNames.Add(number, value);
            myPokemonNumbers.Add(value, number);
          }
        }
      }
    }

    public string GetPokemonName(short number) => myPokemonNames.TryGetValue(number, out var name) ? name: null;
    public short? GetPokemonNumber(string name) => myPokemonNumbers.TryGetValue(name, out var number) ? number : default(short?);
  }
}