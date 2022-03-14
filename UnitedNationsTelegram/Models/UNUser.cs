using BotFramework.Identity;

namespace UnitedNationsTelegram.Models;

public class UNUser : IdentityUser
{
    public List<UserCountry> Countries { get; set; }
}

public class UserCountry
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public int CountryId { get; set; }
    public Country Country { get; set; }
    public long UserId { get; set; }
    public UNUser User { get; set; }

    public List<Vote> Votes { get; set; }
    public List<Poll> OpenedPolls { get; set; }
}

public class Country
{
    public int Id { get; set; }
    public string EmojiFlag { get; set; }
    public string Name { get; set; }

    public virtual List<UserCountry> Users { get; set; }
}

public class Vote
{
    public int UserCountryId { get; set; }
    public int PollId { get; set; }
    public Reaction Reaction { get; set; }

    public virtual Poll Poll { get; set; }
    public virtual UserCountry Country { get; set; }
}

public class Poll
{
    public int Id { get; set; }
    public int OpenedById { get; set; }
    public int MessageId { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;

    public string Text { get; set; }
    public List<Vote> Votes { get; set; }
    public UserCountry OpenedBy { get; set; }
}