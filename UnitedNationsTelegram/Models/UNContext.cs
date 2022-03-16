using BotFramework.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;
using UnitedNationsTelegram.Commands;

namespace UnitedNationsTelegram.Models;

public class UNContext : IdentityDbContext<UNUser>
{
    public DbSet<Sanction> Sanctions { get; set; }
    public DbSet<UserCountry> UserCountries { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Poll> Polls { get; set; }

    public UNContext(DbContextOptions builder) : base(builder)
    {
    }

    public async Task<UserCountry?> FindByCountry(string input, long chatId)
    {
        return await UserCountries
            .Include(a => a.Country)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => EF.Functions.ILike(a.Country.Name, $"%{input}%")
                                      || EF.Functions.ILike(a.Country.EmojiFlag, $"%{input}%")
                                      && a.ChatId == chatId);
    }

    public async Task<int> MembersCount(long chatId)
    {
        return await UserCountries.CountAsync(a => a.ChatId == chatId);
    }

    public async Task<List<UserCountry>> MainMembersNotVoted(long chat, int pollId)
    {
        var mainMembers = await MainMembers(chat);
        var mainMemberNotVoted = mainMembers.Where(a => a.Votes.All(c => c.PollId != pollId)).ToList();
        return mainMemberNotVoted;
    }

    public async Task<List<UserCountry>> MainMembers(long chat)
    {
        return UserCountries.Include(a => a.Votes)
            .Include(a => a.Country)
            .Include(a => a.User)
            .Where(a => a.ChatId == chat).OrderByDescending(a => a.Votes.Count).Take(MainController.MainMembersCount).ToList();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Sanction>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Poll).WithOne(a => a.Sanction).HasForeignKey<Sanction>(a => a.PollId);
            e.HasOne(a => a.Against).WithMany(a => a.Sanctions).HasForeignKey(a => a.AgainstId);
        });

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