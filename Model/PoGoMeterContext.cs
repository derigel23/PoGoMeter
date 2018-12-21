using System;
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

      modelBuilder.Entity<Stats>(builder =>
      {
        builder.HasKey(_ => new { _.Pokemon, _.CP, _.Level, _.AttackIV, _.DefenseIV, _.StaminaIV });
      });

      modelBuilder.Entity<BaseStats>(builder =>
      {
        builder.HasKey(_ => _.Pokemon);
        builder.HasMany(_ => _.Stats).WithOne().HasForeignKey(_ => _.Pokemon);
      });
    }
  }
}
