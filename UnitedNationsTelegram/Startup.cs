using BotFramework.Abstractions;
using BotFramework.Identity;
using BotFramework.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UnitedNationsTelegram.Models;
using UnitedNationsTelegram.Services;

namespace UnitedNationsTelegram;

public static class Startup
{
    public static void Configure(IAppBuilder app, HostBuilderContext context)
    {
        app.Services.AddDbContext<UNContext>(
            builder => builder.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection")));

        app.AddIdentity<UNUser, IdentityRole>()
            .AddEntityFrameworkStores<UNContext>();

        app.Services.AddScoped<PollService>();
        app.Services.AddScoped<SanctionService>();
    }
}