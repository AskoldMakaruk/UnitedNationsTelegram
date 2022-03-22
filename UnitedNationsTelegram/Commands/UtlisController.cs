using System.Text;
using BotFramework.Abstractions;
using BotFramework.Extensions;
using BotFramework.Services.Commands.Attributes;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using UnitedNationsTelegram.Models;
using UnitedNationsTelegram.Services;
using UnitedNationsTelegram.Utils;

namespace UnitedNationsTelegram.Commands;

public class UtlisController : UnController
{
    [Priority(EndpointPriority.First)]
    [StartsWith("/polls")]
    public async Task Polls()
    {
        var polls = await pollService.GetPolls(ChatId, 0);
        var total = await context.Polls.Include(a => a.OpenedBy).CountAsync(a => a.OpenedBy.ChatId == ChatId);

        var text = PollsToString(polls);
        var markup = PollsMarkup(0, 10, total);
        await Client.SendTextMessage(text, parseMode: ParseMode.Html, replyMarkup: markup);
    }

    [Priority(EndpointPriority.First)]
    [CallbackData("polls")]
    public async Task PollsArrows()
    {
        var skip = int.Parse(Update.CallbackQuery?.Data?["polls_".Length..]);
        var polls = await pollService.GetPolls(ChatId, skip);
        var total = await context.Polls.Include(a => a.OpenedBy).CountAsync(a => a.OpenedBy.ChatId == ChatId);

        var text = PollsToString(polls, skip);
        var markup = PollsMarkup(skip - 10, skip + 10, total);

        await Client.EditMessageText(Update.CallbackQuery.Message.MessageId, text, parseMode: ParseMode.Html, replyMarkup: markup);
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/roll_country")]
    public async Task RollCountry()
    {
        var country = context.Countries.Include(a => a.Users).Where(a => a.Users.All(a => a.ChatId != ChatId))
            .ToList().OrderBy(a => Random.Shared.Next()).First();

        await Client.SendTextMessage($"{country.EmojiFlag}{country.Name}");
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/roll_member")]
    public async Task RollMember()
    {
        var member = context.UserCountries.Include(a => a.Country).Include(a => a.User)
            .Where(a => a.ChatId == ChatId)
            .ToList()
            .OrderBy(a => Random.Shared.Next()).First();

        await Client.SendTextMessage($"{member.Country.EmojiFlag}{member.Country.Name} - @{member.User.UserName}");
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/ping")]
    public async Task Ping()
    {
        var country = await CheckUserCountry();
        if (country == null)
        {
            return;
        }

        if (await sanctionService.CheckUserSanction("vote", ChatId, country.UserCountryId))
        {
            await Client.SendTextMessage("На тебе накладено санкції, друже. Ти не можеш пігнувати нікого.",
                replyToMessageId: Update.Message.MessageId);
            return;
        }

        var type = Update.Message?.Text?["/ping".Length..].RemoveBotName();
        string answer;
        if (type == "all")
        {
            answer = await All();
        }
        else if (type == "main")
        {
            answer = await M();
        }
        else if (await context.FindCountry(type, ChatId) is { } a)
        {
            answer = $"Увага {a.Country.EmojiFlag}{a.Country.Name} @{a.User.UserName}";
        }
        else
        {
            answer = $"параметри команди ping \n<b>all<\b> - викликати усіх членів РадБезу ООН\n<b>main<\b> - викликати головних членів РадБезу ООН";
        }

        await Client.SendTextMessage(answer, parseMode: ParseMode.Html);

        async Task<string> All()
        {
            var members = await context.UserCountries.Include(a => a.User).Include(a => a.Country).Where(a => a.ChatId == ChatId).ToListAsync();
            return "Увага усім членам РадБезу ООН\n" + string.Join("\n", members.Select(a => $"{a.Country.EmojiFlag}{a.Country.Name} @{a.User.UserName}"));
        }

        async Task<string> M()
        {
            var members = await context.MainMembers(ChatId);
            return "Увага основним членам РадБезу ООН\n" + string.Join("\n", members.Select(a => $"{a.Country.EmojiFlag}{a.Country.Name} @{a.User.UserName}"));
        }
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/members")]
    public async Task Members()
    {
        // var admins = await bot.GetChatAdministratorsAsync(ChatId);
        // var members = admins.Select(a => CheckUserCountry(ChatId, a.User.UserCountryId)).ToList();
        // await Task.WhenAll(members);

        var builder = new StringBuilder();
        var users = await context.UserCountries
            .Include(a => a.Country)
            .Include(a => a.Votes).ThenInclude(a => a.Poll)
            .Include(a => a.User)
            .Where(a => a.ChatId == ChatId).ToListAsync();

        var polls = users.SelectMany(a => a.Votes.Select(a => a.Poll)).DistinctBy(a => a.PollId).ToList();
        var countries = users.Select(a => a.Country).DistinctBy(a => a.CountryId).ToList();

        builder.Append($"В чаті <b>{Chat.Title}</b> проведено <b>{polls.Count}</b> голосовань у яких було подано <b>{polls.SelectMany(a => a.Votes).Count()}</b> голосів <b>{countries.Count}</b> країнами\n\n");

        var yesterdayVotes = users.Select(a => (Id: a.UserCountryId, Votes: a.Votes.Where(a => a.Created > DateTime.Now.StartOfWeek(DayOfWeek.Monday) && a.Created <= (DateTime.Now - TimeSpan.FromDays(1)).Date).ToList())).ToList();
        var userYsOrder = users.OrderByDescending(a => yesterdayVotes.FirstOrDefault(x => x.Id == a.UserCountryId).Votes.Count).ToList();

        var i = 0;
        builder.AppendLine("Основні члени РадБезу:");
        foreach (var userCountry in users.OrderByDescending(a => a.Votes.Count(a => a.Created.Value > DateTime.Now.StartOfWeek(DayOfWeek.Monday))))
        {
            if (i == MainMembersCount)
            {
                builder.AppendLine($"\nУсі інші члени РадБезу:");
            }

            var previousDayIndex = userYsOrder.IndexOf(userCountry);
            var userCountryVotes = userCountry.Votes.Where(a => a.Created.Value > DateTime.Now.StartOfWeek(DayOfWeek.Monday)).ToList();
            var votesChange = userCountryVotes.Count - yesterdayVotes.FirstOrDefault(a => a.Id == userCountry.UserCountryId).Votes.Count;
            var votesChangeText = votesChange == 0 ? "" : $"(+{votesChange})";
            builder.AppendLine($"{i + 1}{GetChange(i, previousDayIndex)}. {userCountry.Country.EmojiFlag}{userCountry.Country.Name}: <b>{userCountry.User.UserName} - {userCountryVotes.Count} {votesChangeText}</b>");

            i++;
        }

        var result = builder.ToString();
        await Client.SendTextMessage(result, parseMode: ParseMode.Html);

        string GetChange(int previous, int current)
        {
            if (previous > current)
            {
                return $"(-{Math.Abs(current - previous)}↓)";
            }

            if (previous < current)
            {
                return $"(+{current - previous}↑)";
            }

            return "";
        }
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/stats")]
    public async Task Stats()
    {
        var input = Update.Message?.Text?.RemoveBotName()?.Trim();
        UserCountry? country;
        if (input is "/stats" or null)
        {
            country = await CheckUserCountry();
        }
        else
        {
            var search = input["/stats".Length..].Trim();
            country = await context.FindCountry(search, ChatId);
        }

        if (country == null)
        {
            await Client.SendTextMessage("не вдалося ніхуя знайти, друже");
            return;
        }

        var votes = await context.Votes.Where(a => a.UserCountryId == country.UserCountryId).ToListAsync();
        var polls = await pollService.GetUserPolls(country.UserCountryId);
        var date = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
        var userPlace = (await context.UserCountries.Where(a => a.UserCountryId != country.UserCountryId && a.ChatId == country.ChatId).Include(a => a.Votes).ToListAsync()).Count(a => a.Votes.Count(c => c.Created > date) > votes.Count(c => c.Created > date));
        var voteGroups = votes.GroupBy(a => a.Reaction).Select(a => $"{Reactions[a.Key]} - {a.Count()} <b>({(double)a.Count() / votes.Count:P})</b>");
        var mostCommonReactions = polls.SelectMany(a => a.Votes).GroupBy(a => a.Reaction).OrderByDescending(a => a.Count()).Take(3);
        var totalReactions = polls.Sum(a => a.Votes.Count);
        var totalPolls = polls.Where(a => !a.IsActive && a.IsSigned && a.Votes.Count != 0).ToList();
        var pollGroups = totalPolls.GroupBy(a => GetPollResultReaction(a.Votes));

        var friends = polls.SelectMany(a => a.Votes).GroupBy(a => a.Country).Where(a => a.Key != country)
            .OrderByDescending(a => a.Count(c => c.Reaction is Reaction.For or Reaction.Support)).Take(3).ToList();
        var foes = polls.SelectMany(a => a.Votes).GroupBy(a => a.Country).Where(a => a.Key != country)
            .OrderByDescending(a => a.Count(c => c.Reaction is Reaction.Condemn or Reaction.Veto or Reaction.Against)).Take(3).ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"<b>{userPlace + 1}. {country.ToFlagName()}</b> на чолі з {country.User.UserName}");
        builder.AppendLine($"Усього питань піднятих країною: <b>{polls.Count}</b>");
        builder.AppendLine($"Найпоширеніші рекації інших країн на питання:");
        builder.AppendJoin("\n", mostCommonReactions.Select(c => $" <b>{ResultReactions[c.Key]} - {c.Count()} ({(double)c.Count() / totalReactions:P})</b>"));
        builder.AppendLine("\n");
        builder.AppendLine($"Найбільші друзі:");
        builder.AppendJoin("\n", friends.Select(c => $"{c.Key.ToFlagName()} - <b>{c.Count(c => c.Reaction is Reaction.For or Reaction.Support)}</b>"));
        builder.AppendLine("\n");
        builder.AppendLine($"Найзапекліші вороги:");
        builder.AppendJoin("\n", foes.Select(c => $"{c.Key.ToFlagName()} - <b>{c.Count(c => c.Reaction is Reaction.Condemn or Reaction.Veto or Reaction.Against)}</b>"));
        builder.AppendLine("\n");
        builder.AppendLine("<b>Результати питань:</b>");
        builder.AppendJoin("\n", pollGroups.Select(a => $"{(a.Key == null ? "<b>Не вдалося зрозуміти чого хоче РадБез</b>😵‍💫" : Reactions[a.Key.Value])} - {a.Count()} <b>({(double)a.Count() / totalPolls.Count:P})</b>"));
        builder.AppendLine("\n");
        builder.AppendLine($"Усі голоси <b>{votes.Count}</b>:");
        builder.AppendJoin("\n", voteGroups);

        await Client.SendTextMessage(builder.ToString(), parseMode: ParseMode.Html);
    }

    public UtlisController(IClient client, UpdateContext update, ITelegramBotClient bot, UNUser user, UNContext context, SanctionService sanctionService, PollService pollService) : base(client, update, bot, user, context, sanctionService, pollService)
    {
    }
}

public static class DateTimeExtensions
{
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }
}