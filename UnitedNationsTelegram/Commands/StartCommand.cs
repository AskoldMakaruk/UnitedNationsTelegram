using System.Text;
using System.Text.RegularExpressions;
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

[Priority(EndpointPriority.First)]
public class MainController : CommandControllerBase
{
    public static string BotUserName;
    private readonly ITelegramBotClient bot;
    private readonly UNUser user;
    private readonly UNContext context;

    public MainController(IClient client,
        UpdateContext update,
        ITelegramBotClient bot,
        UNUser user,
        UNContext context) : base(client, update)
    {
        this.bot = bot;
        this.user = user;
        this.context = context;
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/start")]
    public async Task Start()
    {
        try
        {
            System.Console.WriteLine(Update.GetInfoFromUpdate().Chat.Id);
            await Client.SendTextMessage("Цей бот є офіційний представник РадБез ООН.\n/vote + текст щоб почати голосування.");
            var chat = Update.GetInfoFromUpdate().Chat;
            var members = await bot.GetChatAdministratorsAsync(Update.GetInfoFromUpdate().Chat);
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine(e);
            throw;
        }
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/vote")]
    public async Task Vote()
    {
        try
        {
            var message = await CheckUserCountry();
            if (message != null)
            {
                await Client.SendTextMessage(message);
                return;
            }


            var pollText = Update.Message?.ReplyToMessage?.Text ?? Update.Message?.Text["/vote".Length..].Replace($"@{BotUserName}", "");

            if (pollText.Length < 3)
            {
                await Client.SendTextMessage("Це занадто мале питання. Придумай щось сурйозніще.");
                return;
            }

            var chat = Update.GetInfoFromUpdate().Chat;
            if (chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
            {
                await Client.SendTextMessage("В цьому чаті неможливо розпочати голосування.");
                return;
            }

            var activePoll = await context.Polls.FirstOrDefaultAsync(a => a.ChatId == chat.Id && a.IsActive);
            if (activePoll != null)
            {
                await Client.SendTextMessage("В цьому чаті вже є активне голосування.", replyToMessageId: activePoll.MessageId);
                return;
            }

            string text = $"{user.Country.EmojiFlag}{user.Country.Name} піднімає питання:\n" +
                          $"{pollText}\n\n" +
                          $"Голосуємо панове.";

            var poll = new UnitedNationsTelegram.Models.Poll()
            {
                Text = pollText,
                Votes = new List<Vote>(),
                ChatId = chat.Id,
                IsActive = true
            };
            context.Polls.Add(poll);
            await context.SaveChangesAsync();

            var keyboard = VoteMarkup(poll.Id);

            var pollMessage = await Client.SendTextMessage(text, replyMarkup: keyboard);
            poll.MessageId = pollMessage.MessageId;
            await context.SaveChangesAsync();
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine(e);
            throw;
        }
    }

    [Priority(EndpointPriority.First)]
    [StartsWith("/close")]
    public async Task ClosePoll()
    {
        var chat = Update.GetId();
        var poll = await context.Polls.Include(c => c.Votes).ThenInclude(c => c.Country).FirstOrDefaultAsync(a => a.ChatId == chat && a.IsActive);
        if (poll == null)
        {
            await Client.SendTextMessage("В цьому чаті немає питань на голосуванні.");
        }
        else
        {
            poll.IsActive = false;
            var results = VotesToString(poll.Votes);
            await Client.SendTextMessage($"Результати голосування: \n{results}", replyToMessageId: poll.MessageId);
            await context.SaveChangesAsync();
        }
    }

    [Priority(EndpointPriority.First)]
    [CallbackData("vote")]
    public async Task CastVote()
    {
        var data = Update.CallbackQuery!.Data!.Split("_");
        var reaction = Enum.Parse<Reaction>(data[1]);
        var pollId = int.Parse(data[2]);

        var message = await CheckUserCountry();
        if (message != null)
        {
            await Client.AnswerCallbackQuery(Update.CallbackQuery.Id, message);
            return;
        }

        var country = user.Country;
        var poll = await context.Polls.Include(a => a.Votes).FirstOrDefaultAsync(a => a.Id == pollId);

        var vote = poll.Votes.FirstOrDefault(a => a.CountryId == country.Id);
        if (vote == null)
        {
            vote = new Vote()
            {
                CountryId = country.Id,
                PollId = poll.Id
            };

            context.Votes.Add(vote);
        }

        vote.Reaction = reaction;

        await context.SaveChangesAsync();

        var pollMessage = Update.CallbackQuery.Message;

        var votes = await context.Votes.Include(a => a.Country).Where(a => a.PollId == pollId).ToListAsync();
        var votesText = VotesToString(votes);

        string text;
        if (pollMessage.Text.Contains("Результати:"))
        {
            text = pollMessage.Text[..pollMessage.Text.IndexOf("\nРезультати:")] + $"\nРезультати:\n{votesText}";
        }
        else
        {
            text = pollMessage.Text + $"\nРезультати:\n{votesText}";
        }

        await Client.EditMessageText(pollMessage.MessageId, text, replyMarkup: pollMessage.ReplyMarkup);
    }

    // [Priority(EndpointPriority.First)]
    // [StartsWith("/promote")]
    // [Admin]
    // public async Task SetTitle()
    // {
    //     var promoted = Update.Message?.ReplyToMessage?.From;
    //     var chat = Update.GetInfoFromUpdate().Chat;

    //     if (promoted == null)
    //     {
    //         await Client.SendTextMessage("не зрозумів кого промотити. а ну пішов звудси");
    //         return;
    //     }

    //     var countryName = Update.Message?.Text?["/promote".Length..].Trim();
    //     if (countryName == null || context.Countries.FirstOrDefault(c => EF.Functions.ILike(c.Name, countryName)) is not { } country)
    //     {
    //         await Client.SendTextMessage("не зрозумів якої країни ти посол блін");
    //         return;
    //     }


    //     await bot.PromoteChatMemberAsync(chat, promoted.Id);
    //     await bot.SetChatAdministratorCustomTitleAsync(chat, promoted.Id, country.Name.ToLower());
    //     await Client.SendTextMessage($"{promoted.Username} тепер посол {country.Name}{country.EmojiFlag}");
    // }

    [Priority(EndpointPriority.First)]
    [StartsWith("/members")]
    public async Task Members()
    {
        var chat = Update.GetInfoFromUpdate().Chat;
        var admins = await bot.GetChatAdministratorsAsync(chat);
        var builder = new StringBuilder();
        var polls = await context.Polls
            .Include(a => a.Votes).ThenInclude(a => a.Country).ThenInclude(a => a.Users)
            .Where(a => a.ChatId == chat.Id).ToListAsync();

        // var members = context.Countries.FirstOrDefault(a => EF.Functions.ILike(a.Name, title));

        var countries = polls.SelectMany(a => a.Votes.Select(a => a.Country)).DistinctBy(a => a.EmojiFlag).ToList();
        builder.AppendFormat("В чаті <b>{0}</b> проведено <b>{1}</b> голосовань у яких було подано <b>{2}</b> голосів <b>{3}</b> країнами\n\n",
            chat.Title, polls.Count, polls.SelectMany(a => a.Votes).Count(),
            countries.Count
        );
        builder.AppendLine("Представники усіх країн:");

        foreach (var country in countries)
        {
            var reprs = admins.Where(a => country.Users.Any(c => c.Id == a.User.Id)).ToList();

            builder.Append($"{country.EmojiFlag}{country.Name} наразі ");
            if (reprs.Count == 0)
            {
                builder.Append("немає жодного представника(");
            }
            else if (reprs.Count == 1)
            {
                var user = reprs[0].User;
                builder.AppendLine($"гордо представляється <b>{user.Username}</b>");
                builder.AppendLine($"усього голосів: <b>{country.Votes.Count}</b>");
                // builder.AppendLine($"піднято питань **{}**", polls.Count(a=>a.))
            }

            builder.AppendLine();
        }

        var result = builder.ToString();
        await Client.SendTextMessage(result, parseMode: ParseMode.Html);
    }

    public async Task<string?> CheckUserCountry()
    {
        var info = Update.GetInfoFromUpdate();
        var chatUser = await bot.GetChatMemberAsync(info.Chat, info.From.Id);
        var title = GetCustomTitle(chatUser);
        if (title == null)
        {
            return "Ви не є членом РадБезу ООН. Зверністься до адміністрації для вступу до Ради Безпеки ООН.";
        }

        if (user.CountryId == null)
        {
            var country = context.Countries.FirstOrDefault(a => EF.Functions.ILike(a.Name, title));
            if (country == null)
            {
                return $"Не існує такої країни як {title}, довбень.";
            }

            user.Country = country;
            await context.SaveChangesAsync();
        }

        if (user.Country == null)
        {
            user.Country = await context.Countries.FindAsync(user.CountryId);
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

    public static List<(Reaction Reaction, string Text)> Reactions => new List<(Reaction, string)>()
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
            $"{ResultReactions.FirstOrDefault(x => x.Reaction == a.Key).Text} {string.Concat(a.Select(c => c.Country.EmojiFlag))}"
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