using BotFramework.Abstractions;

namespace UnitedNationsTelegram.Utils;

public class AdminAttribute : CommandAttribute
{
    public override bool? Suitable(UpdateContext context)
    {
        var userId = context.Update.GetUser()?.Id;
        return userId is 249258727 or 249122421;
    }
}