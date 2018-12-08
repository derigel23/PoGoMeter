using System;

namespace PoGoMeter.Configuration
{
    public class BotConfiguration
    {
      public string BotToken { get; set; }
      public TimeSpan Timeout { get; set; }
    }
}