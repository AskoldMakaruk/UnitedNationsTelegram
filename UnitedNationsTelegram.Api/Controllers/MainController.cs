using Microsoft.AspNetCore.Mvc;
using UnitedNationsTelegram.Services.Models;
using UnitedNationsTelegram.Services.Services;

namespace UnitedNationsTelegram.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class MainController : ControllerBase
{
    private readonly MembersService _membersService;
    private readonly ILogger<MainController> _logger;

    public MainController(MembersService membersService, ILogger<MainController> logger)
    {
        _membersService = membersService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<JsonResult> Get(long chatId)
    {
        _logger.LogInformation("Get members {ChatId}", chatId);
        var member = await _membersService.GetMember(chatId);
        return new JsonResult(await _membersService.GetChatMembers(member.ChatId));
    }
}