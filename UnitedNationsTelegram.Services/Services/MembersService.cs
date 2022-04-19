using Microsoft.EntityFrameworkCore;
using UnitedNationsTelegram.Services.Models;

namespace UnitedNationsTelegram.Services.Services;

public class MembersService
{
    private readonly UNContext _context;

    public MembersService(UNContext context)
    {
        _context = context;
    }

    public async Task<MembersViewModel> GetChatMembers(long chatId)
    {
        var result = new MembersViewModel();

        var users = await _context.UserCountries
            .Include(a => a.Country)
            .Include(a => a.Votes).ThenInclude(a => a.Poll)
            .Include(a => a.User)
            .Where(a => a.ChatId == chatId).ToListAsync();

        var polls = users.SelectMany(a => a.Votes.Select(a => a.Poll)).DistinctBy(a => a.PollId).ToList();

        //result.Polls = polls;

        var yesterdayVotes = users.Select(a => (Id: a.UserCountryId, Votes: a.Votes.Where(a => a.Created > DateTime.Now.StartOfWeek(DayOfWeek.Monday) && a.Created <= (DateTime.Now - TimeSpan.FromDays(1)).Date).ToList())).ToList();
        var userYsOrder = users.OrderByDescending(a => yesterdayVotes.FirstOrDefault(x => x.Id == a.UserCountryId).Votes.Count).ToList();

        var i = 0;
        foreach (var userCountry in users.OrderByDescending(a => a.Votes.Count(a => a.Created.Value > DateTime.Now.StartOfWeek(DayOfWeek.Monday))))
        {
            var previousDayIndex = userYsOrder.IndexOf(userCountry);
            var userCountryVotes = userCountry.Votes.Where(a => a.Created.Value > DateTime.Now.StartOfWeek(DayOfWeek.Monday)).ToList();


            var yesterdayVotesCount = yesterdayVotes.FirstOrDefault(a => a.Id == userCountry.UserCountryId).Votes.Count;

            var member = new MembersViewModel.MemberViewModel()
            {
                Place = i + 1,
                PlaceDelta = i - previousDayIndex,
                IsMain = i < Constants.MainMembersCount,
                Country = userCountry.Country.Name,
                Flag = userCountry.Country.EmojiFlag,
                Name = userCountry.User.UserName,
                UserName = userCountry.User.UserName,
                Votes = userCountryVotes.Count,
                YesterdayVotes = yesterdayVotesCount
            };


            result.Countries.Add(member);
            i++;
        }

        return result;
    }

    public async Task<UserCountry?> GetMember(long chatId)
    {
        return await _context.UserCountries.FirstOrDefaultAsync(a => a.User.Id == chatId);
    }
}

public class MembersViewModel
{
    //public List<Poll> Polls { get; set; } = new();
    public List<MemberViewModel> Countries { get; set; } = new();

    public class MemberViewModel
    {
        public int Place { get; set; }
        public int PlaceDelta { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Country { get; set; }
        public string Flag { get; set; }
        public int Votes { get; set; }
        public int YesterdayVotes { get; set; }
        public bool IsMain { get; set; }
    }
}