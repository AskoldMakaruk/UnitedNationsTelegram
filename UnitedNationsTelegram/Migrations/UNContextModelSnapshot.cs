﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UnitedNationsTelegram.Models;

#nullable disable

namespace UnitedNationsTelegram.Migrations
{
    [DbContext(typeof(UNContext))]
    partial class UNContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BotFramework.Identity.IdentityRole", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("BFRoles", (string)null);
                });

            modelBuilder.Entity("BotFramework.Identity.IdentityRoleClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<long>("RoleId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("BFRoleClaims", (string)null);
                });

            modelBuilder.Entity("BotFramework.Identity.IdentityUserClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("BF_UserClaims", (string)null);
                });

            modelBuilder.Entity("BotFramework.Identity.IdentityUserRole", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<long>("RoleId")
                        .HasColumnType("bigint");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("BFUserRoles", (string)null);
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("EmojiFlag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EmojiFlag")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.Poll", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<int>("MessageId")
                        .HasColumnType("integer");

                    b.Property<int>("OpenedById")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("OpenedById");

                    b.ToTable("Polls");
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.UNUser", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.ToTable("BF_Users", (string)null);
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.UserCountry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<int>("CountryId")
                        .HasColumnType("integer");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("CountryId", "ChatId", "UserId")
                        .IsUnique();

                    b.ToTable("UserCountries");
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.Vote", b =>
                {
                    b.Property<int>("UserCountryId")
                        .HasColumnType("integer");

                    b.Property<int>("PollId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Reaction")
                        .HasColumnType("integer");

                    b.HasKey("UserCountryId", "PollId");

                    b.HasIndex("PollId");

                    b.ToTable("Votes");
                });

            modelBuilder.Entity("BotFramework.Identity.IdentityRoleClaim", b =>
                {
                    b.HasOne("BotFramework.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BotFramework.Identity.IdentityUserClaim", b =>
                {
                    b.HasOne("UnitedNationsTelegram.Models.UNUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BotFramework.Identity.IdentityUserRole", b =>
                {
                    b.HasOne("BotFramework.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UnitedNationsTelegram.Models.UNUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.Poll", b =>
                {
                    b.HasOne("UnitedNationsTelegram.Models.UserCountry", "OpenedBy")
                        .WithMany("OpenedPolls")
                        .HasForeignKey("OpenedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OpenedBy");
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.UserCountry", b =>
                {
                    b.HasOne("UnitedNationsTelegram.Models.Country", "Country")
                        .WithMany("Users")
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UnitedNationsTelegram.Models.UNUser", "User")
                        .WithMany("Countries")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Country");

                    b.Navigation("User");
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.Vote", b =>
                {
                    b.HasOne("UnitedNationsTelegram.Models.Poll", "Poll")
                        .WithMany("Votes")
                        .HasForeignKey("PollId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("UnitedNationsTelegram.Models.UserCountry", "Country")
                        .WithMany("Votes")
                        .HasForeignKey("UserCountryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Country");

                    b.Navigation("Poll");
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.Country", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.Poll", b =>
                {
                    b.Navigation("Votes");
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.UNUser", b =>
                {
                    b.Navigation("Countries");
                });

            modelBuilder.Entity("UnitedNationsTelegram.Models.UserCountry", b =>
                {
                    b.Navigation("OpenedPolls");

                    b.Navigation("Votes");
                });
#pragma warning restore 612, 618
        }
    }
}
