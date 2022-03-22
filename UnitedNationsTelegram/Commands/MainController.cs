using BotFramework.Abstractions;
using BotFramework.Extensions;
using BotFramework.Services.Commands.Attributes;
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
                await Client.SendTextMessage("Не вдалося зрозуміти текст питання.");
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