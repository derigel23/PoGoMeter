﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PoGoMeter.Model;

namespace PoGoMeter.Migrations
{
    [DbContext(typeof(PoGoMeterContext))]
    partial class PoGoMeterContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("PoGoMeter")
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PoGoMeter.Model.BaseStats", b =>
                {
                    b.Property<short>("Pokemon")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<short>("Attack");

                    b.Property<short>("Defense");

                    b.Property<short>("Stamina");

                    b.HasKey("Pokemon");

                    b.ToTable("BaseStats");
                });

            modelBuilder.Entity("PoGoMeter.Model.Stats", b =>
                {
                    b.Property<short>("Pokemon");

                    b.Property<short>("CP");

                    b.Property<byte>("Level");

                    b.Property<byte>("AttackIV");

                    b.Property<byte>("DefenseIV");

                    b.Property<byte>("StaminaIV");

                    b.HasKey("Pokemon", "CP", "Level", "AttackIV", "DefenseIV", "StaminaIV");

                    b.ToTable("Stats");
                });

            modelBuilder.Entity("PoGoMeter.Model.Stats", b =>
                {
                    b.HasOne("PoGoMeter.Model.BaseStats")
                        .WithMany("Stats")
                        .HasForeignKey("Pokemon")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
