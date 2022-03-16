using BotFramework.Abstractions;

namespace UnitedNationsTelegram.Utils;

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