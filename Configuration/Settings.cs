using System.Collections.Generic;

namespace PoGoMeter.Configuration
{ 
  public class Settings
  {
    public byte MinIV { get; set; }
    public Dictionary<short, byte> SpecialMinIVCheck { get; set; }
    public byte BestBuddyMinIV { get; set; }
    public byte BestBuddyMinLevel { get; set; }
  }
}