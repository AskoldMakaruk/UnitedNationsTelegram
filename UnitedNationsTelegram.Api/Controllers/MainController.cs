using Microsoft.AspNetCore.Mvc;
using UnitedNationsTelegram.Services.Models;
using UnitedNationsTelegram.Services.Services;

namespace UnitedNationsTelegram.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class MainController : ControllerBase
{
    private readonly MembersService _membersService;

    public MainController(MembersService membersService)
    {
        _membersService = membersService;
    }

    [HttpGet]
    public async Task<JsonResult> Get(long chatId)
    {
        var member = await _membersService.GetMember(chatId);
        return new JsonResult(await _membersService.GetChatMembers(member.ChatId));
    }
}