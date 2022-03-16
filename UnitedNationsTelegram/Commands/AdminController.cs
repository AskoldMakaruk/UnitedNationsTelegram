using BotFramework.Abstractions;
using BotFramework.Extensions;
using BotFramework.Services.Commands.Attributes;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using UnitedNationsTelegram.Models;
using UnitedNationsTelegram.Services;
using UnitedNationsTelegram.Utils;

namespace UnitedNationsTelegram.Commands;

[Admin]
public class AdminController : UnController
{
    public AdminController(IClient client,
        UpdateContext update,
        ITelegramBotClient bot,
        UNUser user,
        UNContext context,
        SanctionService sanctionService,
        PollService pollService
    ) : base(client, update, bot, user, context, sanctionService, pollService)
    {
    }

    [Admin]
    [Priority(EndpointPriority.First)]
    [StartsWith("/update")]
    public async Task SendUpdate()
    {
        var message = Update.Message;
        var chats = await context.UserCountries.Select(a => a.ChatId).Distinct().ToListAsync();

        foreach (var chat in chats)
        {
            try
            {
                await Client.ForwardMessage(ChatId, message.MessageId, chatId: chat);
            }
            catch (Exception e)
            {
            }
        }
    }
}