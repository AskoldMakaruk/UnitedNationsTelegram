using System.Security.Cryptography;
using System.Text;

namespace UnitedNationsTelegram.Api;

public class TelegramDataValidator : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Request.EnableBuffering();
        var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;
        
        //generate secret key with HMAC_SHA256
        var secretKey = "WebAppData";
        var hash = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var botKey = hash.ComputeHash(Encoding.UTF8.GetBytes("823973981:AAHEBgfxE8juepArApUGZtmD4QVbJ8ZIJEY"));
        
        
        
        // secret_key = HMAC_SHA256(<bot_token>, "WebAppData")
        // if (hex(HMAC_SHA256(data_check_string, secret_key)) == hash) {
        //     // data is from Telegram
        // }

        await next(context);
    }
    
    //validate HMAC_SHA256 signature
    private bool ValidateSignature(string data, string signature)
    {
        var secretKey = "WebAppData";
        var hash = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var botKey = hash.ComputeHash(Encoding.UTF8.GetBytes("823973981:AAHEBgfxE8juepArApUGZtmD4QVbJ8ZIJEY"));
        var hashString = BitConverter.ToString(botKey).Replace("-", "").ToLower();
        var dataHash = new HMACSHA256(Encoding.UTF8.GetBytes(data));
        var dataHashString = BitConverter.ToString(dataHash.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", "").ToLower();
        return dataHashString == signature;
    }
    
}