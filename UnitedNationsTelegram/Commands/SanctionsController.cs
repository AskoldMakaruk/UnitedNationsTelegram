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
public class SanctionsController : UnController
{
    [Priority(EndpointPriority.First)]
    [StartsWith("/sanction")]
    public async Task CreateSanction()
    {
        var words = Update.Message?.Text?.RemoveBotName()?.Split(' ');

        if (words == null || words.Length < 3)
        {
            await Client.SendTextMessage("Щоб створити голосування на накладення санкцій потрібно" +
                                         " відправити тип санкції та ім'я або прапор країни на яку ці санкції будуть накладені.\n" +
                                         "Наприклад: \n" +
                                         "<code>/sanctions veto сербія</code>\n" +
                                         "або\n" +
                                         "<code>/sanctions ping сербія</code>\n\n" +
                                         "Типи санкцій:\n" +
                                         "<code>veto</code> - забороняє країні накладати вето\n" +
                                         "<code>vote</code> - забороняє країні створювати питання\n" +
                                         "<code>ping</code> - забороняє країні блять пінгувати усіх\n",
                parseMode: ParseMode.Html
            );
            return;
        }

        var country = await CheckUserCountry();
        if (country == null)
        {
            return;
        }

        var sanctionType = words[1];
        if (!SanctionTypes.Contains(sanctionType))
        {
            await Client.SendTextMessage($"Не вдалося зрозуміти тип санкцій {sanctionType}");
            return;
        }

        var findInput = words[2];
        var sanctionCountry = await context.FindByCountry(findInput, ChatId);
        if (sanctionCountry == null)
        {
            await Client.SendTextMessage($"Не вдалося знайти країну {findInput}");
            return;
        }

        var poll = new Poll
        {
            Text = GetSanctionPollText(sanctionType, sanctionCountry),
            Type = PollType.Sanction,
            IsActive = true,
            OpenedBy = country
        };

        var sanction = new Sanction
        {
            Against = sanctionCountry,
            Poll = poll,
            SanctionType = sanctionType,
            IsSupported = false
        };

        context.Sanctions.Add(sanction);
        await AddPoll(poll);
    }


    public SanctionsController(IClient client, UpdateContext update, ITelegramBotClient bot, UNUser user, UNContext context, SanctionService sanctionService, PollService pollService) : base(client, update, bot, user, context, sanctionService, pollService)
    {
    }
}