﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TheDialgaTeam.Worktips.Discord.Bot.Services.EntityFramework;

namespace TheDialgaTeam.Worktips.Discord.Bot.Migrations
{
    [DbContext(typeof(SqliteContext))]
    [Migration("20190501155436_0.0.1")]
    partial class _001
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity("TheDialgaTeam.Worktips.Discord.Bot.EntityFramework.WalletAccount", b =>
                {
                    b.Property<int>("WalletAccountId")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("AccountIndex");

                    b.Property<string>("RegisteredWalletAddress");

                    b.Property<string>("TipWalletAddress");

                    b.Property<ulong>("UserId");

                    b.HasKey("WalletAccountId");

                    b.ToTable("WalletAccountTable");
                });
#pragma warning restore 612, 618
        }
    }
}
