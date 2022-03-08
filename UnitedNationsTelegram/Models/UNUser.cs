using BotFramework.Identity;
using BotFramework.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace UnitedNationsTelegram.Models;

public class UNUser : IdentityUser
{
    public int? CountryId { get; set; }
    public Country? Country { get; set; }
}

public class Country
{
    public int Id { get; set; }
    public string EmojiFlag { get; set; }
    public string Name { get; set; }

    public virtual List<Vote> Votes { get; set; }
    public virtual List<UNUser> Users { get; set; }

}

public enum Reaction
{
    // 👍 - за
    For,
    // 👎 - проти
    Against,
    // 👏 - підтримати
    Support,
    // 😡 - засудити
    Condemn,
    // 🤔 - не прийти
    Absent,
    // 😢 - стурбованість
    Concern,
    // 🤮 - вето
    Veto,
}

public class Vote
{
    public int CountryId { get; set; }
    public int PollId { get; set; }
    public Reaction Reaction { get; set; }

    public virtual Poll Poll { get; set; }
    public virtual Country Country { get; set; }
}
public class Poll
{
    public int Id { get; set; }
    public string Text { get; set; }
    public virtual List<Vote> Votes { get; set; }
}

public class UNContext : IdentityDbContext<UNUser>
{
    public DbSet<Country> Countries { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Poll> Polls { get; set; }

    public UNContext(DbContextOptions builder) : base(builder)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Vote>((e) =>
        {
            e.HasKey(a => new { a.CountryId, a.PollId });
        });


        builder.Entity<Poll>((e) =>
        {
            e.HasMany(a => a.Votes).WithOne(a => a.Poll).HasForeignKey(a => a.PollId);
        });

        builder.Entity<Country>((e) =>
        {
            e.HasMany(a => a.Votes).WithOne(a => a.Country).HasForeignKey(a => a.CountryId);
            e.HasIndex(a => a.Name).IsUnique();
            e.HasIndex(a => a.EmojiFlag).IsUnique();
        });

        builder.Entity<UNUser>((e) =>
        {
            e.HasOne(a => a.Country).WithMany(a => a.Users).HasForeignKey(a => a.CountryId);
        });

        base.OnModelCreating(builder);
    }
}