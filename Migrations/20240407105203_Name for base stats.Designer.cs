﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PoGoMeter.Model;

#nullable disable

namespace PoGoMeter.Migrations
{
    [DbContext(typeof(PoGoMeterContext))]
    [Migration("20240407105203_Name for base stats")]
    partial class Nameforbasestats
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("PoGoMeter")
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("PoGoMeter.Model.BaseStats", b =>
                {
                    b.Property<short>("Pokemon")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<short>("Pokemon"), 1L, 1);

                    b.Property<short>("Attack")
                        .HasColumnType("smallint");

                    b.Property<short>("Defense")
                        .HasColumnType("smallint");

                    b.Property<decimal>("Height")
                        .HasColumnType("decimal(9,3)");

                    b.Property<short>("Stamina")
                        .HasColumnType("smallint");

                    b.Property<decimal>("Weight")
                        .HasColumnType("decimal(9,3)");

                    b.HasKey("Pokemon");

                    b.ToTable("BaseStats", "PoGoMeter");
                });

            modelBuilder.Entity("PoGoMeter.Model.Ignore", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<short>("Pokemon")
                        .HasColumnType("smallint");

                    b.HasKey("UserId", "Pokemon");

                    b.HasIndex("UserId");

                    b.ToTable("Ignore", "PoGoMeter");
                });

            modelBuilder.Entity("PoGoMeter.Model.PokemonName", b =>
                {
                    b.Property<short>("Pokemon")
                        .HasColumnType("smallint");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Pokemon");

                    b.ToTable("PokemonName", "PoGoMeter");
                });

            modelBuilder.Entity("PoGoMeter.Model.Stats", b =>
                {
                    b.Property<short>("Pokemon")
                        .HasColumnType("smallint");

                    b.Property<short>("CP")
                        .HasColumnType("smallint");

                    b.Property<byte>("Level")
                        .HasColumnType("tinyint");

                    b.Property<byte>("AttackIV")
                        .HasColumnType("tinyint");

                    b.Property<byte>("DefenseIV")
                        .HasColumnType("tinyint");

                    b.Property<byte>("StaminaIV")
                        .HasColumnType("tinyint");

                    b.HasKey("Pokemon", "CP", "Level", "AttackIV", "DefenseIV", "StaminaIV");

                    b.ToTable("Stats", "PoGoMeter");
                });

            modelBuilder.Entity("PoGoMeter.Model.PokemonName", b =>
                {
                    b.HasOne("PoGoMeter.Model.BaseStats", null)
                        .WithOne("Name")
                        .HasForeignKey("PoGoMeter.Model.PokemonName", "Pokemon")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PoGoMeter.Model.Stats", b =>
                {
                    b.HasOne("PoGoMeter.Model.BaseStats", null)
                        .WithMany("Stats")
                        .HasForeignKey("Pokemon")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PoGoMeter.Model.BaseStats", b =>
                {
                    b.Navigation("Name");

                    b.Navigation("Stats");
                });
#pragma warning restore 612, 618
        }
    }
}
