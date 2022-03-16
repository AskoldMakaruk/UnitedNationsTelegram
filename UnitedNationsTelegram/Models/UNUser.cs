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
    public List<Sanction> Sanctions { get; set; }

    public string ToFlagName() => $"{Country.EmojiFlag}{Country.Name}";
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
    public DateTime? Created { get; set; } = DateTime.Now;

    public virtual Poll Poll { get; set; }
    public virtual UserCountry Country { get; set; }
}

public class Sanction
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public int AgainstId { get; set; }
    public string SanctionType { get; set; }
    public DateTime? ActiveUntil { get; set; }
    public bool IsSupported { get; set; }

    public UserCountry Against { get; set; }
    public Poll Poll { get; set; }
}

public class Poll
{
    public int Id { get; set; }
    public int OpenedById { get; set; }
    public int MessageId { get; set; }
    public bool IsActive { get; set; }
    public bool IsSigned { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;

    public string Text { get; set; }
    public PollType Type { get; set; }

    public UserCountry OpenedBy { get; set; }
    public Sanction Sanction { get; set; }
    public List<Vote> Votes { get; set; } = new();
    public List<Signature> Signatures { get; set; } = new List<Signature>();
}

public class Signature
{
    public int Id { get; set; }
    public int UserCountryId { get; set; }
    public int PollId { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;

    public Poll Poll { get; set; }
    public UserCountry UserCountry { get; set; }
}

public enum PollType
{
    Normal = 1,
    Sanction = 2,
    FlagChange = 3,
    NameChange = 4,
    AddCountry = 5,
    DeleteCountry = 6
}