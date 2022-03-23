using System.Text;
using BotFramework.Abstractions;
using BotFramework.Extensions;
using BotFramework.Services.Commands;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UnitedNationsTelegram.Models;
using UnitedNationsTelegram.Services;
using Poll = UnitedNationsTelegram.Models.Poll;
using PollType = UnitedNationsTelegram.Models.PollType;

namespace UnitedNationsTelegram.Commands;

public abstract class UnController : CommandControllerBase
{
    public const int MinMembersVotes = 15;
    public const int MainMembersCount = 7;
    public const int SignatureRequirement = 5;

    protected readonly ITelegramBotClient bot;
    protected readonly UNUser user;
    protected readonly UNContext context;
    protected readonly SanctionService sanctionService;
    protected readonly PollService pollService;

    protected readonly Chat Chat;
    protected readonly long ChatId;


    public UnController(IClient client,
        UpdateContext update,
        ITelegramBotClient bot,
        UNUser user,
        UNContext context,
        SanctionService sanctionService, PollService pollService) : base(client, update)
    {
        this.bot = bot;
        this.user = user;
        this.context = context;
        this.sanctionService = sanctionService;
        this.pollService = pollService;

        Chat = update.Update.GetInfoFromUpdate().Chat;
        ChatId = Chat.Id;
    }

    public async Task SendPoll(Poll poll, bool forceSend = false, long? chatId = null)
    {
        var ch = chatId ?? poll.OpenedBy.ChatId;
        var country = poll.OpenedBy;
        var votesText = VotesToString(poll.Votes);
        var count = await context.Polls.CountAsync(a => a.OpenedBy.ChatId == poll.OpenedBy.ChatId && !a.IsActive && a.IsSigned);

        var text = $"{country.Country.EmojiFlag}{country.Country.Name} піднімає питання під номером <b>{count + 1}</b>:\n" +
                   $"{poll.Text}\n\n" +
                   $"Голосуємо панове.";

        if (poll.Votes.Count != 0)
        {
            text += $"\nГолоси:\n{votesText}";
        }

        var mainMemberNotVoted = await context.MainMembersNotVoted(ch, poll.PollId);
        var canClose = poll.Votes.Count >= MinMembersVotes || mainMemberNotVoted.Count == 0;

        text += $"\nМожливо закрити: <b>{(canClose ? "так✅" : "ні❌")}</b>\n";

        if (!canClose)
        {
            text += $"Мінімальна кількість голосів: ({poll.Votes.Count} &lt; {MinMembersVotes})\nЩе не проголосували: {string.Concat(mainMemberNotVoted.Select(a => a.Country.EmojiFlag))}";
        }

        var keyboard = VoteMarkup();
        if (forceSend || Update.Message != null)
        {
            if (poll.MessageId == 0 || forceSend)
            {
                var pollMessage = await Client.SendTextMessage(text, replyMarkup: keyboard, parseMode: ParseMode.Html, chatId: ch);
                if (Chat.Type != ChatType.Private)
                {
                    poll.MessageId = pollMessage.MessageId;
                }
            }
            else
            {
                await Client.SendTextMessage("Ось тут ☝️", replyToMessageId: poll.MessageId);
            }

            if (forceSend)
            {
                var chatMembers = await context.UserCountries.Include(a => a.User).Where(a => a.ChatId == ch && a.UserId != user.Id).ToListAsync();
                foreach (var c in chatMembers)
                {
                    try
                    {
                        await Client.SendTextMessage(text, c.User.Id, replyMarkup: keyboard, parseMode: ParseMode.Html);
                    }
                    catch
                    {
                        //ignore
                    }
                }
            }
        }

        if (!forceSend && Update.CallbackQuery != null)
        {
            var pollMessage = Update.CallbackQuery.Message;

            await Client.EditMessageText(pollMessage.MessageId, text, replyMarkup: pollMessage.ReplyMarkup, parseMode: ParseMode.Html);
            if (pollMessage.MessageId != poll.MessageId)
            {
                await Client.EditMessageText(poll.MessageId, text, ch, replyMarkup: pollMessage.ReplyMarkup, parseMode: ParseMode.Html);
            }

            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, "Ваш голос прийнято!");
        }

        await context.SaveChangesAsync();

        InlineKeyboardMarkup VoteMarkup()
        {
            var buttons = Reactions
                .Where(a => poll.Type != PollType.Sanction || a.Key != Reaction.Veto)
                .Select(a => new InlineKeyboardButton(a.Value)
                {
                    CallbackData = $"vote_{a.Key}_{poll.PollId}",
                })
                .Chunk(3);

            return new InlineKeyboardMarkup(buttons);
        }
    }

    public async Task<UserCountry?> CheckUserCountry()
    {
        var info = Update.GetInfoFromUpdate();
        return await CheckUserCountry(info.Chat.Id, info.From.Id);
    }

    public async Task<UserCountry?> CheckUserCountryWithChatId(long chatId)
    {
        UserCountry? country = null;
        if (ChatId != user.Id)
        {
            country = await CheckUserCountry();
        }
        else
        {
            country ??= await context.UserCountries.Include(a => a.Country).Include(a => a.User).FirstOrDefaultAsync(a => a.ChatId == chatId && a.UserId == user.Id);
        }

        return country;
    }

    public async Task<UserCountry?> CheckUserCountry(long chatId, long userId)
    {
        string? message = null;
        var chatUser = await bot.GetChatMemberAsync(chatId, userId);
        var title = GetCustomTitle(chatUser);
        if (title == null)
        {
            message = "Ви не є членом РадБезу ООН. Зверністься до адміністрації для вступу до Ради Безпеки ООН.";
        }

        user.Countries = await context.UserCountries
            .Include(a => a.Country).Include(a => a.Votes)
            .Where(a => a.ChatId == chatId && a.UserId == user.Id).ToListAsync();

        var userCountry = user.Countries
            .FirstOrDefault(a => a.ChatId == chatId && string.Equals(a.Country.Name, title, StringComparison.OrdinalIgnoreCase));

        if (userCountry == null && title != null)
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

            foreach (var toRemove in user.Countries.Where(a => a.ChatId == chatId && !string.Equals(a.Country.Name, title, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                user.Countries.Remove(toRemove);
                context.Remove(toRemove);
            }

            await context.SaveChangesAsync();
        }

        if (message == null) return userCountry;

        if (Update.CallbackQuery != null)
        {
            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, message, true);
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

    public static readonly IReadOnlyDictionary<Reaction, string> Reactions = new Dictionary<Reaction, string>()
    {
        [Reaction.For] = "За 👍",
        [Reaction.Against] = "Проти 👎",
        [Reaction.Support] = "Підтримати 👏",
        [Reaction.Condemn] = "Засудити 😡",
        [Reaction.Absent] = "Не прийти 🤔",
        [Reaction.Concern] = "Стурбованість 😢",
        [Reaction.Veto] = "Вето 🤮",
    };

    public static readonly IReadOnlyDictionary<Reaction, string> ResultReactions = new Dictionary<Reaction, string>()
    {
        [Reaction.For] = "За 👍",
        [Reaction.Against] = "Проти 👎",
        [Reaction.Support] = "Підтримали 👏",
        [Reaction.Condemn] = "Засудили 😡",
        [Reaction.Absent] = "Не прийшли 🤔",
        [Reaction.Concern] = "Стурбовані 😢",
        [Reaction.Veto] = "Наклали вето 🤮",
    };

    public static string PollsToString(List<Poll> polls, int skipCount = 0)
    {
        var builder = new StringBuilder();
        if (skipCount != 0)
        {
            builder.AppendLine($"Питання від {skipCount} до {skipCount + polls.Count}:");
        }
        else
        {
            builder.AppendLine($"Останні {polls.Count} питань:");
        }

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
                builder.AppendLine($"{GetPollResult(poll.Votes)}\n");
            }
        }

        return builder.ToString();
    }


    public static InlineKeyboardMarkup PollsMarkup(int from, int to, int max)
    {
        var list = new List<InlineKeyboardButton>();

        if (from >= 0)
        {
            list.Add(new InlineKeyboardButton("<< сюди")
            {
                CallbackData = $"polls_{from}"
            });
        }

        if (max > 10 && to <= max)
        {
            list.Add(new InlineKeyboardButton("туди >>")
            {
                CallbackData = $"polls_{Math.Min(to, max)}"
            });
        }

        return new InlineKeyboardMarkup(list);
    }

    public static string VotesToString(List<Vote> votes)
    {
        if (votes.Count == 0)
        {
            return "";
        }

        var builder = new StringBuilder();
        var reactionLines = votes.OrderByDescending(a => a.Reaction).ThenByDescending(a => a.Created).GroupBy(a => a.Reaction).Select(a =>
            $"{ResultReactions[a.Key]} {string.Concat(a.Select(c => c.Country.Country.EmojiFlag))}"
        ).ToList();
        builder.AppendJoin("\n", reactionLines);
        builder.AppendLine("\n\nРезультат:");
        builder.AppendLine(GetPollResult(votes));

        return builder.ToString();
    }

    public static Reaction? GetPollResultReaction(List<Vote> votes)
    {
        var reactions = new List<List<Reaction>>()
        {
            new() { Reaction.Absent },
            new() { Reaction.Against },
            new() { Reaction.Concern },
            new() { Reaction.Condemn },
            new() { Reaction.For, Reaction.Support },
        };

        var vetos = votes.Where(a => a.Reaction == Reaction.Veto).ToList();
        var count = votes.Count / 2;

        if (vetos.Count != 0)
        {
            return Reaction.Veto;
        }

        foreach (var list in reactions)
        {
            if (Check(list) is { } res)
            {
                return res;
            }
        }

        return null;


        Reaction? Check(List<Reaction> reaction)
        {
            var votesCount = votes.Count(a => reaction.Contains(a.Reaction));
            return votesCount > count ? reaction[0] : null;
        }
    }

    public static string GetPollResult(List<Vote> votes)
    {
        if (votes.Count == 0)
        {
            return "";
        }

        var reactions = new List<(List<Reaction> reactions, string Text)>()
        {
            (new() { Reaction.Absent }, "<b>Ніхто не прийшов на вечірку</b>🥱"),
            (new() { Reaction.Against }, "Рішення <b>не прийнято</b>❌"),
            (new() { Reaction.Concern }, "Рішення <b>відправляємо занепокоєння</b>😢"),
            (new() { Reaction.Condemn }, "Рішення <b>відправляємо засудження</b>😾"),
            (new() { Reaction.For, Reaction.Support }, "Рішення <b>прийнято</b>✅"),
        };

        var vetos = votes.Where(a => a.Reaction == Reaction.Veto).ToList();
        var count = votes.Count / 2;

        if (vetos.Count != 0)
        {
            var plural = (votes.Count > 1 ? "и" : "а");
            return $"Країн{plural} наклал{plural} {Reactions[Reaction.Veto]}\n{string.Concat(vetos.Select(a => a.Country.Country.EmojiFlag))}";
        }

        foreach (var (list, text) in reactions)
        {
            if (Check(list, text) is { } res)
            {
                return res;
            }
        }

        return "<b>Не вдалося зрозуміти чого хоче РадБез</b>😵‍💫";


        string? Check(List<Reaction> reaction, string result)
        {
            var votesCount = votes.Count(a => reaction.Contains(a.Reaction));
            return votesCount > count ? result : null;
        }
    }

    public async Task AddPoll(Poll poll)
    {
        var res = await pollService.AddPoll(poll);
        if (res.errorMessage != null)
        {
            await Client.SendTextMessage(res.errorMessage, parseMode: ParseMode.Html);
        }
        else
        {
            await SendPollPetition(res.pollToSend!);
        }
    }

    public async Task SendPollPetition(Poll poll)
    {
        var chatId = poll.OpenedBy.ChatId;
        var str = new StringBuilder();
        str.AppendLine($"{poll.OpenedBy.ToFlagName()} збирає підписи щоб підняти питання:");
        str.AppendLine(poll.Text);
        str.AppendLine();
        if (poll.Signatures.Count > 0)
        {
            str.AppendLine($"Підписали: {string.Concat(poll.Signatures.Select(a => a.UserCountry.Country.EmojiFlag))}");
        }

        var count = await context.MembersCount(chatId);
        count = Math.Min(count - 1, SignatureRequirement);
        if (poll.Signatures.Count < count)
        {
            str.AppendLine($"Залишилось підписів: {count - poll.Signatures.Count}");
        }


        var text = str.ToString();
        if (Update.Message != null)
        {
            await Client.SendTextMessage(text, parseMode: ParseMode.Html, replyMarkup: PetitionMarkup(poll, count));
            var chatMembers = await context.UserCountries.Include(a => a.User).Where(a => a.ChatId == chatId && a.UserId != user.Id).ToListAsync();
            foreach (var c in chatMembers)
            {
                try
                {
                    await Client.SendTextMessage(text, c.User.Id, replyMarkup: PetitionMarkup(poll, count), parseMode: ParseMode.Html);
                }
                catch
                {
                    //ignore
                }
            }
        }

        var collectedSignatures = count <= poll.Signatures.Count;

        if (collectedSignatures)
        {
            poll.IsActive = true;
            poll.IsSigned = true;
        }

        if (Update.CallbackQuery != null)
        {
            var pollMessage = Update.CallbackQuery.Message;

            if (pollMessage.Text != text.Trim().ReplaceLineEndings("\n") || collectedSignatures)
            {
                await Client.EditMessageText(pollMessage.MessageId, text, replyMarkup: PetitionMarkup(poll, count), parseMode: ParseMode.Html);
            }

            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, "Ваш підпис прийнято!");
        }

        if (poll.Signatures.Count >= count)
        {
            var activePolls = await context.Polls.Include(a => a.OpenedBy)
                .Where(a => a.IsActive && a.OpenedBy.ChatId == chatId && a.PollId != poll.PollId)
                .CountAsync();
            if (activePolls != 0)
            {
                await Client.SendTextMessage($"Після збору підписів у чергу під номером <b>{activePolls}</b> було додане питання від <b>{poll.OpenedBy.ToFlagName()}</b>:\n{poll.Text}",
                    parseMode: ParseMode.Html,
                    chatId: chatId
                );
                await context.SaveChangesAsync();
                return;
            }

            await SendPoll(poll, true);
        }
    }

    public static InlineKeyboardMarkup? PetitionMarkup(Poll poll, int count)
    {
        if (poll.Signatures.Count >= count)
        {
            return null;
        }

        return new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Підписати✍️", $"sign_{poll.PollId}"));
    }

    public string GetSanctionPollCloseText(Poll poll)
    {
        var against = poll.Sanction.Against;
        var type = poll.Sanction.SanctionType;
        var date = poll.Sanction.ActiveUntil?.ToString() ?? "навічно нахуй";

        return $"Зусилями {Chat.Title} {against.ToFlagName()} було заборонено {GetSanctionActionText(type)} до {date}";
    }

    public string GetSanctionPollText(string type, UserCountry against)
    {
        return $"<b>Заборонити {against.ToFlagName()} {GetSanctionActionText(type)}.</b>";
    }

    public string GetSanctionActionText(string type)
    {
        return type switch
        {
            "veto" => $"накладати вето",
            "vote" => $"піднімати питання",
            "ping" => $"пінгувати людей",
            _ => null!
        };
    }


    public static readonly IReadOnlyCollection<string> SanctionTypes = new[]
    {
        "veto",
        "vote",
        "ping"
    };
}