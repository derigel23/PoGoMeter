using Microsoft.EntityFrameworkCore;

namespace PoGoMeter.Model
{
  public class PoGoMeterContext : DbContext
  {
    public PoGoMeterContext(DbContextOptions<PoGoMeterContext> options)
      : base(options) { }

    public DbSet<Stats> Stats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.HasDefaultSchema("PoGoMeter");

      var builder = modelBuilder.Entity<Stats>();

      builder.HasKey(stats => new { stats.Pokemon, stats.CP,  stats.Level, stats.AttackIV, stats.DefenseIV, stats.StaminaIV });
    }
  }
}
