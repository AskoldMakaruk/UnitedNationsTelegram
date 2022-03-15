using Microsoft.EntityFrameworkCore;
using UnitedNationsTelegram.Models;

namespace UnitedNationsTelegram.Services;

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

        var pollsFromUserCount = await context.Polls
            .Include(a => a.OpenedBy).ThenInclude(a => a.User)
            .CountAsync(a => a.OpenedBy.ChatId == ChatId && a.OpenedBy.User.Id == userId && a.IsActive);

        if (pollsFromUserCount >= 2)
        {
            return (null, "Ти вже додав нормальну кількість питань у чергу.");
        }

        context.Polls.Add(poll);
        await context.SaveChangesAsync();

        var activePolls = await context.Polls.Include(a => a.OpenedBy)
            .Where(a => a.IsActive && a.OpenedBy.ChatId == ChatId && a.Id != poll.Id)
            .CountAsync();
        if (activePolls != 0)
        {
            return (null, $"В цьому чаті вже є активне голосування.\nТвоє питання поставлено у чергу під номером <b>{activePolls}</b>:\n{poll.Text}");
        }

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
            .Where(a => a.OpenedBy.ChatId == chatId)
            .OrderByDescending(a => a.Created)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Poll?> GetActivePoll(long chatId)
    {
        return await Polls.FirstOrDefaultAsync(a => a.OpenedBy.ChatId == chatId && a.IsActive && a.MessageId != 0);
    }

    private IQueryable<Poll> Polls => context.Polls
        .Include(a => a.OpenedBy).ThenInclude(a => a.Country)
        .Include(a => a.Votes).ThenInclude(a => a.Country).ThenInclude(a => a.Country)
        .Include(a => a.Sanction).ThenInclude(a => a.Against).ThenInclude(a => a.Country);
}