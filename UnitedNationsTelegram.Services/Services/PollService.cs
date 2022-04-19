using Microsoft.EntityFrameworkCore;
using UnitedNationsTelegram.Services.Models;

namespace UnitedNationsTelegram.Services.Services;

public class PollService
{
    private readonly UNContext context;

    public PollService(UNContext context)
    {
        this.context = context;
    }

    public async Task<(Poll? pollToSend, string? errorMessage)> AddPoll(Poll poll)
    {
        var ChatId = poll.OpenedBy.ChatId;
        var userId = poll.OpenedBy.User.Id;
        poll.IsSigned = false;
        poll.IsActive = false;

        // var pollsFromUserCount = await context.Polls
        //     .Include(a => a.OpenedBy).ThenInclude(a => a.User)
        //     .CountAsync(a => a.OpenedBy.ChatId == ChatId
        //                      && a.OpenedBy.User.Id == userId
        //                      && a.IsActive);
        //
        // if (pollsFromUserCount >= 2)
        // {
        //     return (null, "Ти вже додав нормальну кількість питань у чергу.");
        // }

        context.Polls.Add(poll);
        await context.SaveChangesAsync();

        return (poll, null);
    }

    public async Task<Poll?> GetNextPoll(long chatId)
    {
        return await Polls
            .OrderBy(a => a.Created)
            .FirstOrDefaultAsync(a => a.OpenedBy.ChatId == chatId && a.IsActive && a.MessageId == 0);
    }

    public async Task<List<Poll>> GetPolls(long chatId, int skip, int take = 10)
    {
        return await Polls
            .Where(a => a.OpenedBy.ChatId == chatId && a.IsSigned)
            .OrderByDescending(a => a.Created)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Poll?> GetActivePoll(long chatId)
    {
        return await Polls.FirstOrDefaultAsync(a => a.OpenedBy.ChatId == chatId && a.IsActive && a.MessageId != 0);
    }

    public async Task<Poll?> GetPoll(int pollId)
    {
        return await Polls.FirstOrDefaultAsync(a => a.PollId == pollId);
    }

    public async Task<List<Poll>> GetUserPolls(int userId)
    {
        return await Polls.Where(a => a.OpenedById == userId).ToListAsync();
    }

    private IQueryable<Poll> Polls => context.Polls
        .Include(a => a.OpenedBy).ThenInclude(a => a.Country)
        .Include(a => a.Votes).ThenInclude(a => a.Country).ThenInclude(a => a.Country)
        .Include(a => a.Sanction).ThenInclude(a => a.Against).ThenInclude(a => a.Country)
        .Include(a => a.Signatures).ThenInclude(a => a.UserCountry).ThenInclude(a => a.Country);
}