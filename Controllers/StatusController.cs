using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PoGoMeter.Controllers
{
  public class StatusController : Team23.TelegramSkeleton.StatusController
  {
    private readonly CalculatorCP myCalculator;
    public StatusController(ITelegramBotClient bot, CalculatorCP calculator) : base(bot)
    {
      myCalculator = calculator;
    }

    protected override async Task<dynamic> GetStatusData(CancellationToken cancellationToken)
    {
      var statusData = await base.GetStatusData(cancellationToken);
      statusData.MinIV = myCalculator.MinIV;
      statusData.MaxIV = myCalculator.MaxIV;
      return statusData;
    }
  }
}