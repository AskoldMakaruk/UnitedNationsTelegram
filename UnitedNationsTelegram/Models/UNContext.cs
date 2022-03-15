using BotFramework.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace UnitedNationsTelegram.Models;

public class UNContext : IdentityDbContext<UNUser>
{
    public DbSet<UserCountry> UserCountries { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Poll> Polls { get; set; }

    public UNContext(DbContextOptions builder) : base(builder)
    {
    }

    public async Task<UserCountry?> FindByCountryName(string input, long chatId)
    {
        return await UserCountries
            .Include(a => a.Country)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => EF.Functions.ILike(a.Country.Name, $"%{input}%") && a.ChatId == chatId);
    }

    public async Task<Poll?> GetNextPoll(long ChatId)
    {
        return await Polls
            .Include(a => a.OpenedBy).ThenInclude(a => a.Country)
            .Include(a => a.Votes).ThenInclude(a => a.Country).ThenInclude(a => a.Country)
            .OrderBy(a => a.Created)
            .FirstOrDefaultAsync(a => a.OpenedBy.ChatId == ChatId && a.IsActive && a.MessageId == 0);
    }

    public async Task<List<Poll>> GetPolls(long ChatId, int skip, int take = 10)
    {
        return await Polls
            .Include(a => a.OpenedBy).ThenInclude(a => a.Country)
            .Include(a => a.Votes)
            .ThenInclude(a => a.Country)
            .Where(a => a.OpenedBy.ChatId == ChatId)
            .OrderByDescending(a => a.Created)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
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