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
using Poll = UnitedNationsTelegram.Models.Poll;
using PollType = UnitedNationsTelegram.Models.PollType;

namespace UnitedNationsTelegram.Commands;

[Priority(EndpointPriority.First)]
public class MainController : UnController
{
    [Priority(EndpointPriority.First)]
    [StartsWith("/start")]
    public async Task Start()
    {
        await Client.SendTextMessage("Цей бот є офіційний представник РадБез ООН.\n/vote + текст щоб почати голосування.");
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/vote")]
    public async Task Vote()
    {
        try
        {
            var country = await CheckUserCountry();
            if (country == null)
            {
                return;
            }

            var pollText = "";
            if (Update.Message?.ReplyToMessage?.Text != null && Update.Message?.ReplyToMessage?.From?.IsBot == false)
            {
                pollText = Update.Message.ReplyToMessage.Text;
            }
            else if (Update.Message?.Text?["/vote".Length..] is { } text)
            {
                pollText = text;
            }

            pollText = pollText.RemoveBotName()!;

            if (pollText.Length < 3)
            {
                await Client.SendTextMessage("Це занадто мале питання. Придумай щось сурйозніще.");
                return;
            }

            if (Chat.Type == ChatType.Private)
            {
                await Client.SendTextMessage("В цьому чаті неможливо розпочати голосування.");
                return;
            }

            if (await sanctionService.CheckUserSanction("vote", ChatId, country.UserCountryId))
            {
                await Client.SendTextMessage("На тебе накладено санкції, друже. Ти не можеш накладати створювати нові питання.",
                    replyToMessageId: Update.Message.MessageId);
                return;
            }

            var poll = new Poll
            {
                Text = pollText,
                Type = PollType.Normal,
                IsActive = true,
                OpenedBy = country,
            };


            await AddPoll(poll);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/close")]
    public async Task ClosePoll()
    {
        var poll = await pollService.GetActivePoll(ChatId);

        if (poll == null)
        {
            await Client.SendTextMessage("В цьому чаті немає питань на голосуванні.");
            return;
        }

        var mainMemberNotVoted = await context.MainMembersNotVoted(ChatId, poll.PollId);
        var canClose = poll.Votes.Count >= MinMembersVotes || mainMemberNotVoted.Count == 0;

        if (canClose)
        {
            var specialText = "";
            if (poll.Type == PollType.Sanction && GetPollResult(poll.Votes).Contains('✅'))
            {
                poll.Sanction.IsSupported = true;
                poll.Sanction.ActiveUntil = DateTime.Now + TimeSpan.FromDays(3);

                specialText = GetSanctionPollCloseText(poll);
            }

            poll.IsActive = false;
            var results = VotesToString(poll.Votes);
            await Client.SendTextMessage($"{specialText}\nПитання: {poll.Text}\n\nГолоси: \n{results}", replyToMessageId: poll.MessageId, parseMode: ParseMode.Html);
            await context.SaveChangesAsync();

            var nextPoll = await pollService.GetNextPoll(ChatId);

            if (nextPoll == null)
            {
                return;
            }

            await SendPoll(nextPoll);
        }
        else
        {
            var s = string.Join(",", mainMemberNotVoted.Select(a => $"{a.Country.EmojiFlag}{a.Country.Name} - @{a.User.UserName}"));
            await Client.SendTextMessage($"Не виконані умови закриття:\nКількість голосів менша за необхідну ({poll.Votes.Count} < {MinMembersVotes})\nНе всі основні країни проголосували ({s}) ", replyToMessageId: poll.MessageId);
        }
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/active")]
    public async Task ActivePoll()
    {
        var poll = await pollService.GetActivePoll(ChatId) ?? await pollService.GetNextPoll(ChatId);

        if (poll == null)
        {
            await Client.SendTextMessage("В цьому чаті немає питань на голосуванні.");
            return;
        }

        await SendPoll(poll);
    }

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
        else if (await context.FindByCountry(type, ChatId) is { } a)
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
    [CallbackData("vote")]
    public async Task CastVote()
    {
        var data = Update.CallbackQuery!.Data!.Split("_");
        var reaction = Enum.Parse<Reaction>(data[1]);
        var pollId = int.Parse(data[2]);

        var country = await CheckUserCountry();
        if (country == null)
        {
            return;
        }

        var poll = await pollService.GetPoll(pollId);

        if (poll is not { IsActive: true })
        {
            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, "Це голосування вже завершено");
            return;
        }

        if (reaction == Reaction.Veto && await sanctionService.CheckUserSanction("veto", ChatId, country.UserCountryId))
        {
            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, "На тебе накладено санкції, друже. Ти не можеш накладати вето.");
            return;
        }

        var vote = poll.Votes.FirstOrDefault(a => a.UserCountryId == country.UserCountryId);
        if (vote == null)
        {
            vote = new Vote()
            {
                UserCountryId = country.UserCountryId,
                PollId = poll.PollId
            };

            if (poll.Votes == null)
            {
                poll.Votes = new List<Vote>();
            }

            poll.Votes.Add(vote);
        }

        vote.Reaction = reaction;
        await context.SaveChangesAsync();
        await SendPoll(poll);
    }

    [Priority(EndpointPriority.First)]
    [CallbackData("sign")]
    public async Task Sign()
    {
        var data = Update.CallbackQuery!.Data!.Split("_");
        var pollId = int.Parse(data[1]);

        var country = await CheckUserCountry();
        if (country == null)
        {
            return;
        }

        var poll = await pollService.GetPoll(pollId);
        if (poll.OpenedById == country.UserCountryId)
        {
            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, "Та ти не можеш підписати своє питання.");
            return;
        }

        if (await sanctionService.CheckUserSanction("vote", ChatId, country.UserCountryId))
        {
            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, "На тебе накладено санкції, друже. Ти не можеш підписувати питання.");
            return;
        }

        var signature = poll.Signatures.FirstOrDefault(a => a.UserCountryId == country.UserCountryId);
        if (signature != null)
        {
            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, "Ти вже підписав це.");
        }
        else
        {
            signature = new Signature()
            {
                PollId = pollId,
                UserCountry = country
            };

            poll.Signatures.Add(signature);
        }

        await context.SaveChangesAsync();
        await SendPollPetition(poll);
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

        var yesterdayVotes = users.Select(a => (Id: a.UserCountryId, Votes: a.Votes.Where(a => a.Created <= (DateTime.Now - TimeSpan.FromDays(1)).Date).ToList())).ToList();
        var userYsOrder = users.OrderByDescending(a => yesterdayVotes.FirstOrDefault(x => x.Id == a.UserCountryId).Votes.Count).ToList();

        var i = 0;
        builder.AppendLine("Основні члени РадБезу:");
        foreach (var userCountry in users.OrderByDescending(a => a.Votes.Count))
        {
            if (i == MainMembersCount)
            {
                builder.AppendLine($"\nУсі інші члени РадБезу:");
            }

            var previousDayIndex = userYsOrder.IndexOf(userCountry);
            var votesChange = userCountry.Votes.Count - yesterdayVotes.FirstOrDefault(a => a.Id == userCountry.UserCountryId).Votes.Count;
            var votesChangeText = votesChange == 0 ? "" : $"(+{votesChange})";
            builder.AppendLine($"{i + 1}{GetChange(i, previousDayIndex)}. {userCountry.Country.EmojiFlag}{userCountry.Country.Name}: <b>{userCountry.User.UserName} - {userCountry.Votes.Count} {votesChangeText}</b>");

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


    public MainController(IClient client,
        UpdateContext update,
        ITelegramBotClient bot,
        UNUser user,
        UNContext context,
        SanctionService sanctionService,
        PollService pollService
    ) : base(client, update, bot, user, context, sanctionService, pollService)
    {
    }
}