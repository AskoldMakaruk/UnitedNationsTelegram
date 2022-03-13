using BotFramework.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Types;
using UnitedNationsTelegram;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var hostBuilder = Host.CreateDefaultBuilder(args)
    .UseConfigurationWithEnvironment()
    .UseSerilog((context, configuration) =>
    {
        configuration
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .WriteTo.Console();
    })
    .UseSimpleBotFramework(Startup.Configure);

hostBuilder.ConfigureAppConfiguration((hostingContext, config) =>
{
    var env = hostingContext.HostingEnvironment;
    Console.WriteLine(env.EnvironmentName);
});
var host = hostBuilder.Build();
var bot = host.Services.GetService<ITelegramBotClient>()!;

var me = bot.GetMeAsync().GetAwaiter().GetResult();
MainController.BotUserName = me.Username;

bot.SetMyCommandsAsync(new List<BotCommand>
{
    new()
    {
        Command = "/start",
        Description = "старт ураа!"
    },
    new()
    {
        Command = "/vote",
        Description = "почати голосування"
    },
    new()
    {
        Command = "/close",
        Description = "завершити голосування"
    },
    new()
    {
        Command = "/members",
        Description = "усі представники країн в цьому чаті"
    },
    new()
    {
        Command = "/polls",
        Description = "останні 10 питань"
    },
    new()
    {
        Command = "/ping",
        Description = "викликати членів РадБезу"
    }
});


host.Run();

//name: "[A-Za-z ]*",\s*emoji: "...."
//https://github.com/risan/country-flag-emoji/blob/master/src/data.js