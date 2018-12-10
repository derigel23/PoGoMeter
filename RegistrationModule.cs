using Autofac;
using Team23.TelegramSkeleton;
using Module = Autofac.Module;

namespace PoGoMeter
{
  public class RegistrationModule : Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      builder.RegisterType<Pokemons>().SingleInstance();
      
      builder.RegisterTelegramSkeleton();
    }
  }
}