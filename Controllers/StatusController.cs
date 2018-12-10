using Telegram.Bot;

namespace PoGoMeter.Controllers
{
  public class StatusController : Team23.TelegramSkeleton.StatusController
  {
    public StatusController(ITelegramBotClient bot) : base(bot) { }
  }
}