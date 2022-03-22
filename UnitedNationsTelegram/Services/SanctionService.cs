using Microsoft.EntityFrameworkCore;
using UnitedNationsTelegram.Models;

namespace UnitedNationsTelegram.Services;

public class SanctionService
{
    private readonly UNContext _context;

    public SanctionService(UNContext context)
    {
        _context = context;
    }


    public async Task<bool> CheckUserSanction(string type, long chatId, int userCountryId)
    {
        type = type.ToLower();
        return await _context.Sanctions
            .AnyAsync(a => a.SanctionType == type
                && a.Against.ChatId == chatId
                && a.Against.UserCountryId == userCountryId
                && (a.ActiveUntil == null || a.ActiveUntil > DateTime.Now)
                && a.IsSupported
            );
    }
}