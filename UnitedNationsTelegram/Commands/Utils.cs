namespace UnitedNationsTelegram.Commands;

public static class Utils
{
    public static string BotUserName;

    public static string? RemoveBotName(this string? s)
    {
        return s?.Replace($"@{BotUserName}", "", StringComparison.InvariantCultureIgnoreCase)?.Trim();
    }
}