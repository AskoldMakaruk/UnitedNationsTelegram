using BotFramework.Identity;
using BotFramework.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace UnitedNationsTelegram.Models;

public class UNUser : IdentityUser
{
    public List<UserCountry> Countries { get; set; }
}

public class UserCountry
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public int CountryId { get; set; }
    public Country Country { get; set; }
    public long UserId { get; set; }
    public UNUser User { get; set; }

    public List<Vote> Votes { get; set; }
    public List<Poll> OpenedPolls { get; set; }
}

public class Country
{
    public int Id { get; set; }
    public string EmojiFlag { get; set; }
    public string Name { get; set; }

    public virtual List<UserCountry> Users { get; set; }
}

public class Vote
{
    public int UserCountryId { get; set; }
    public int PollId { get; set; }
    public Reaction Reaction { get; set; }

    public virtual Poll Poll { get; set; }
    public virtual UserCountry Country { get; set; }
}

public class Poll
{
    public int Id { get; set; }
    public int OpenedById { get; set; }
    public int MessageId { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;

    public string Text { get; set; }
    public List<Vote> Votes { get; set; }
    public UserCountry OpenedBy { get; set; }
}

public class UNContext : IdentityDbContext<UNUser>
{
    public DbSet<UserCountry> UserCountries { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Poll> Polls { get; set; }

    public UNContext(DbContextOptions builder) : base(builder)
    {
        Database.Migrate();
    }

    public async Task<Poll?> GetActivePoll(long chatId)
    {
        return await Polls
            .Include(a => a.OpenedBy)
            .ThenInclude(a => a.Country)
            .Include(c => c.Votes)
            .ThenInclude(c => c.Country)
            .ThenInclude(c => c.Country)
            .FirstOrDefaultAsync(a => a.OpenedBy.ChatId == chatId && a.IsActive && a.MessageId != 0);
    }

    public async Task<List<UserCountry>> MainMembers(long chat)
    {
        return UserCountries.Include(a => a.Votes).Include(a => a.Country).Where(a => a.ChatId == chat).OrderByDescending(a => a.Votes.Count).Take(5).ToList();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Vote>(e =>
        {
            e.HasKey(a => new { a.UserCountryId, a.PollId });
            e.HasOne(a => a.Country).WithMany(a => a.Votes).HasForeignKey(a => a.UserCountryId);
        });


        builder.Entity<Poll>(e =>
        {
            e.HasMany(a => a.Votes).WithOne(a => a.Poll).HasForeignKey(a => a.PollId);
            e.HasOne(a => a.OpenedBy).WithMany(a => a.OpenedPolls).HasForeignKey(a => a.OpenedById);
        });

        builder.Entity<Country>(e =>
        {
            e.HasIndex(a => a.Name).IsUnique();
            e.HasIndex(a => a.EmojiFlag).IsUnique();
        });

        builder.Entity<UserCountry>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => new { a.CountryId, a.ChatId, a.UserId }).IsUnique();
            e.HasOne(a => a.User).WithMany(a => a.Countries).HasForeignKey(a => a.UserId);
            e.HasOne(a => a.Country).WithMany(a => a.Users).HasForeignKey(a => a.CountryId);
        });

        builder.Entity<UNUser>(e => { e.HasMany(a => a.Countries).WithOne(a => a.User).HasForeignKey(a => a.UserId); });

        base.OnModelCreating(builder);
    }
}