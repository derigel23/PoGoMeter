using Microsoft.EntityFrameworkCore;

namespace PoGoMeter.Model
{
  public class PoGoMeterContext : DbContext
  {
    public PoGoMeterContext(DbContextOptions<PoGoMeterContext> options)
      : base(options) { }

    public DbSet<Stats> Stats { get; set; }
    public DbSet<Ignore> Ignore { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.HasDefaultSchema("PoGoMeter");

      modelBuilder.Entity<Stats>(builder =>
      {
        builder.HasKey(t => new { t.Pokemon, t.CP, t.Level, t.AttackIV, t.DefenseIV, t.StaminaIV });
      });

      modelBuilder.Entity<BaseStats>(builder =>
      {
        builder.HasKey(t => t.Pokemon);
        builder.HasMany(t => t.Stats).WithOne().HasForeignKey(t => t.Pokemon);
        builder.Property(t => t.Height).HasColumnType("decimal(9,3)");
        builder.Property(t => t.Weight).HasColumnType("decimal(9,3)");
      });

      modelBuilder.Entity<PokemonName>(builder =>
      {
        builder.HasKey(t => t.Pokemon);
        builder.Property(t => t.Pokemon).ValueGeneratedNever();
      });
      
      modelBuilder.Entity<Ignore>(builder =>
      {
        builder.HasKey(t => new { t.UserId, t.Pokemon });
        builder.HasIndex(t => t.UserId);
      });
    }
  }
}
