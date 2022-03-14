using System.Text;
using BotFramework.Abstractions;
using BotFramework.Extensions;
using BotFramework.Services.Commands;
using BotFramework.Services.Commands.Attributes;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UnitedNationsTelegram.Models;
using Poll = UnitedNationsTelegram.Models.Poll;

[Priority(EndpointPriority.First)]
public class MainController : CommandControllerBase
{
    public static string BotUserName;
    private readonly ITelegramBotClient bot;
    private readonly UNUser user;
    private readonly UNContext context;

    private readonly Chat Chat;
    private readonly long ChatId;

    public MainController(IClient client,
        UpdateContext update,
        ITelegramBotClient bot,
        UNUser user,
        UNContext context) : base(client, update)
    {
        this.bot = bot;
        this.user = user;
        this.context = context;

        Chat = update.Update.GetInfoFromUpdate().Chat;
        ChatId = Chat.Id;
    }

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

            var pollText = Update.Message?.ReplyToMessage?.Text ?? Update.Message?.Text["/vote".Length..].Replace($"@{BotUserName}", "").Trim();

            if (pollText.Length < 3)
            {
                await Client.SendTextMessage("Це занадто мале питання. Придумай щось сурйозніще.");
                return;
            }

            var chat = Update.GetInfoFromUpdate().Chat;
            if (chat.Type == ChatType.Private)
            {
                await Client.SendTextMessage("В цьому чаті неможливо розпочати голосування.");
                return;
            }


            var poll = new Poll()
            {
                Text = pollText,
                Votes = new List<Vote>(),
                OpenedBy = country,
                IsActive = true
            };
            context.Polls.Add(poll);
            await context.SaveChangesAsync();


            var activePoll = await context.Polls.Include(a => a.OpenedBy).Where(a => a.IsActive && a.OpenedBy.ChatId == chat.Id && a.MessageId != 0).CountAsync();
            if (activePoll != 0)
            {
                var text = $"В цьому чаті вже є активне голосування.\nТвоє питання поставлено у чергу під номером <b>{activePoll}</b>";

                await Client.SendTextMessage(text, parseMode: ParseMode.Html);
            }
            else
            {
                await SendPoll(poll);
            }
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
        var chat = Update.GetInfoFromUpdate().Chat!.Id;
        var poll = await context.GetActivePoll(chat);

        if (poll == null)
        {
            await Client.SendTextMessage("В цьому чаті немає питань на голосуванні.");
            return;
        }

        var mainMembers = await context.MainMembers(chat);
        var mainMemberNotVoted = mainMembers.Where(a => a.Votes.All(c => c.PollId != poll.Id)).ToList();
        var mainMembersVoted = mainMemberNotVoted.Count == 0;
        var enoughVotes = poll.Votes.Count >= 8;

        if (enoughVotes || mainMembersVoted)
        {
            poll.IsActive = false;
            var results = VotesToString(poll.Votes);
            await Client.SendTextMessage($"Питання: {poll.Text}\n\nРезультати: \n{results}", replyToMessageId: poll.MessageId);
            await context.SaveChangesAsync();

            var nextPoll = await context.Polls.Include(a => a.OpenedBy).ThenInclude(a => a.Country)
                .OrderByDescending(a => a.Created)
                .FirstOrDefaultAsync(a => a.OpenedBy.ChatId == chat && a.IsActive);

            if (nextPoll == null)
            {
                return;
            }

            await SendPoll(nextPoll);
        }
        else
        {
            var s = string.Join(",", mainMemberNotVoted.Select(a => $"{a.Country.EmojiFlag}{a.Country.Name} - @{a.User.UserName}"));
            await Client.SendTextMessage($"Не виконані умови закриття:\nКількість голосів менша за необхідну ({poll.Votes.Count} < 8)\nНе всі основні країни проголосували ({s}) ", replyToMessageId: poll.MessageId);
        }
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/active")]
    public async Task ActivePoll()
    {
        var chat = Update.GetInfoFromUpdate().Chat!.Id;
        var poll = await context.GetActivePoll(chat);

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
        var polls = await context.Polls
            .Include(a => a.OpenedBy).ThenInclude(a => a.Country)
            .Include(a => a.Votes)
            .ThenInclude(a => a.Country)
            .Where(a => a.OpenedBy.ChatId == ChatId)
            .OrderByDescending(a => a.Created).Take(10).ToListAsync();

        var builder = new StringBuilder();
        builder.AppendLine($"Останні {polls.Count} питань:");

        var future = polls.Where(a => a.IsActive && a.MessageId == 0).ToList();
        var present = polls.Where(a => a.IsActive && a.MessageId != 0).ToList();
        var past = polls.Where(a => !a.IsActive).ToList();

        AddPollList(future, "<b>Черга:</b>");
        AddPollList(present, "<b>Активні:</b>");
        AddPollList(past, "<b>Архів:</b>");

        void AddPollList(List<Poll> list, string name)
        {
            if (list.Count == 0)
            {
                return;
            }

            builder.AppendLine(name);
            foreach (var poll in list)
            {
                builder.AppendLine($"{poll.OpenedBy.Country.EmojiFlag}<b>{poll.OpenedBy.Country.Name}</b> {poll.Created}\n{poll.Text}");
            }

            builder.AppendLine();
        }

        await Client.SendTextMessage(builder.ToString(), parseMode: ParseMode.Html);
    }

    [Priority(EndpointPriority.Last)]
    [StartsWith("/roll")]
    public async Task Roll()
    {
        await Client.SendTextMessage($"<b>{Random.Shared.Next(0, 100)}</b>", parseMode: ParseMode.Html, replyToMessageId: Update.Message.MessageId);
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
        var county = await CheckUserCountry();
        if (county == null)
        {
            return;
        }

        var type = Update.Message?.Text?["/ping".Length..]?.Trim();
        var answer = type switch
        {
            "all" => await All(),
            "main" => await M(),
            _ => $"параметри команди ping \n<b>all<\b> - викликати усіх членів РадБезу ООН\n<b>main<\b> - викликати головних членів РадБезу ООН"
        };

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

        var poll = await context.Polls
            .Include(a => a.OpenedBy).ThenInclude(a => a.Country)
            .Include(a => a.Votes).ThenInclude(a => a.Country)
            .FirstOrDefaultAsync(a => a.Id == pollId);

        var vote = poll.Votes.FirstOrDefault(a => a.UserCountryId == country.Id);
        if (vote == null)
        {
            vote = new Vote()
            {
                UserCountryId = country.Id,
                PollId = poll.Id
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
    [StartsWith("/members")]
    public async Task Members()
    {
        // var admins = await bot.GetChatAdministratorsAsync(chat);
        var builder = new StringBuilder();
        var users = await context.UserCountries
            .Include(a => a.Country)
            .Include(a => a.Votes).ThenInclude(a => a.Poll)
            .Include(a => a.User)
            .Where(a => a.ChatId == ChatId).ToListAsync();

        var polls = users.SelectMany(a => a.Votes.Select(a => a.Poll)).DistinctBy(a => a.Id).ToList();
        var countries = users.Select(a => a.Country).DistinctBy(a => a.Id).ToList();

        builder.AppendFormat("В чаті <b>{0}</b> проведено <b>{1}</b> голосовань у яких було подано <b>{2}</b> голосів <b>{3}</b> країнами\n\n",
            Chat.Title, polls.Count, polls.SelectMany(a => a.Votes).Count(),
            countries.Count
        );

        var i = 0;
        builder.AppendLine("Основні члени РадБезу:");
        foreach (var userCountry in users.OrderByDescending(a => a.Votes.Count))
        {
            if (i == 4)
            {
                builder.AppendLine($"\nУсі інші члени РадБезу:");
            }

            builder.AppendLine($"{userCountry.Country.EmojiFlag}{userCountry.Country.Name} представник: <b>{userCountry.User.UserName} - {userCountry.Votes.Count}</b>");

            i++;
        }

        var result = builder.ToString();
        await Client.SendTextMessage(result, parseMode: ParseMode.Html);
    }

    public async Task SendPoll(Poll poll)
    {
        var country = poll.OpenedBy;
        var votesText = VotesToString(poll.Votes);

        var text = $"{country.Country.EmojiFlag}{country.Country.Name} піднімає питання:\n" +
                   $"{poll.Text}\n\n" +
                   $"Голосуємо панове.";

        if (poll.Votes.Count != 0)
        {
            text += $"\nРезультати:\n{votesText}";
        }

        var keyboard = VoteMarkup(poll.Id);
        if (Update.Message != null)
        {
            var pollMessage = await Client.SendTextMessage(text, replyMarkup: keyboard);
            poll.MessageId = pollMessage.MessageId;
        }

        if (Update.CallbackQuery != null)
        {
            var pollMessage = Update.CallbackQuery.Message;

            await Client.EditMessageText(pollMessage.MessageId, text, replyMarkup: pollMessage.ReplyMarkup);
            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, "Ваш голос прийнято!");
        }

        await context.SaveChangesAsync();
    }

    public async Task<UserCountry?> CheckUserCountry()
    {
        string message = null;
        var info = Update.GetInfoFromUpdate();
        var chatUser = await bot.GetChatMemberAsync(info.Chat, info.From.Id);
        var title = GetCustomTitle(chatUser);
        if (title == null)
        {
            message = "Ви не є членом РадБезу ООН. Зверністься до адміністрації для вступу до Ради Безпеки ООН.";
        }

        user.Countries = await context.UserCountries.Include(a => a.Country).Include(a => a.Votes).Where(a => a.ChatId == info.Chat.Id && a.UserId == user.Id).ToListAsync();
        await context.Entry(user).Collection(a => a.Countries).LoadAsync();
        var userCountry = user.Countries.FirstOrDefault(a => a.ChatId == info.Chat.Id);
        if (userCountry == null)
        {
            var country = context.Countries.FirstOrDefault(a => EF.Functions.ILike(a.Name, title));
            if (country == null)
            {
                message = $"Не існує такої країни як {title}, довбень.";
            }

            user.Countries.Add(new UserCountry()
            {
                Country = country,
                User = user,
                ChatId = ChatId
            });
            await context.SaveChangesAsync();
        }

        if (message == null) return userCountry;

        if (Update.CallbackQuery != null)
        {
            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, message);
        }

        if (Update.Message != null)
        {
            await Client.SendTextMessage(message);
        }

        return null;
    }

    public static string? GetCustomTitle(ChatMember chatMember)
    {
        if (chatMember is ChatMemberAdministrator administrator)
        {
            return administrator.CustomTitle;
        }

        if (chatMember is ChatMemberOwner owner)
        {
            return owner.CustomTitle;
        }

        return null;
    }

    public static List<(Reaction Reaction, string Text)> Reactions => new()
    {
        (Reaction.For, "За 👍"),
        (Reaction.Against, "Проти 👎"),
        (Reaction.Support, "Підтримати 👏"),
        (Reaction.Condemn, "Засудити 😡"),
        (Reaction.Absent, "Не прийти 🤔"),
        (Reaction.Concern, "Стурбованість 😢"),
        (Reaction.Veto, "Вето 🤮"),
    };

    public static readonly IReadOnlyList<(Reaction Reaction, string Text)> ResultReactions = new List<(Reaction, string)>()
    {
        (Reaction.For, "За 👍"),
        (Reaction.Against, "Проти 👎"),
        (Reaction.Support, "Підтримали 👏"),
        (Reaction.Condemn, "Засудили 😡"),
        (Reaction.Absent, "Не прийшли 🤔"),
        (Reaction.Concern, "Стурбовані 😢"),
        (Reaction.Veto, "Наклали вето 🤮"),
    };

    public static InlineKeyboardMarkup VoteMarkup(int voteId)
    {
        return new InlineKeyboardMarkup(Reactions.Select(a => new InlineKeyboardButton(a.Text)
            {
                CallbackData = $"vote_{a.Reaction}_{voteId}",
            })
            .Chunk(3));
    }

    public static string VotesToString(List<Vote> votes)
    {
        var votesText = string.Join("\n", votes.GroupBy(a => a.Reaction).Select(a =>
            $"{ResultReactions.FirstOrDefault(x => x.Reaction == a.Key).Text} {string.Concat(a.Select(c => c.Country.Country.EmojiFlag))}"
        ));
        return votesText;
    }
}

public class CallbackDataAttribute : CommandAttribute
{
    public CallbackDataAttribute(string text)
    {
        Text = text;
    }

    public string Text { get; }

    public override bool? Suitable(UpdateContext context)
    {
        return context.Update.CallbackQuery?.Data?.StartsWith(Text);
    }
}

public class AdminAttribute : CommandAttribute
{
    public override bool? Suitable(UpdateContext context)
    {
        var userId = context.Update.GetUser()?.Id;
        return userId == 249258727 || userId == 249122421;
    }
}