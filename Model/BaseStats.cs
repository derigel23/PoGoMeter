using System.Collections.Generic;

namespace PoGoMeter.Model
{
  public class BaseStats
  {
    public short Pokemon { get; set; }
    public short Attack { get; set; }
    public short Defense { get; set; }
    public short Stamina { get; set; }
    
    public List<Stats> Stats { get; set; }
  }
}