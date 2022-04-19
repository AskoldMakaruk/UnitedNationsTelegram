using BotFramework.Abstractions;
using BotFramework.Extensions;
using BotFramework.Services.Commands.Attributes;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using UnitedNationsTelegram.Bot.Utils;
using UnitedNationsTelegram.Services.Models;
using UnitedNationsTelegram.Services.Services;

namespace UnitedNationsTelegram.Bot.Commands;

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


    [Admin]
    [Priority(EndpointPriority.First)]
    [StartsWith("webapp")]
    public async Task WebApp()
    {
        var text = Update.Message.Text.Replace("webapp", "");
        await Client.SendTextMessage("message", replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton("webapp") { WebApp = new WebAppInfo() { Url = text.Trim() } }){ResizeKeyboard = true});
    }
}