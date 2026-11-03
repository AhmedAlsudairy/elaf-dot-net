using System.Text.Json;

namespace Munaqasat.Web.Domain.Entities;

public class ChatRoom : IHasTimestamps
{
    public string Id { get; set; } = Ids.New();
    public string? TenderId { get; set; }
    public Tender? Tender { get; set; }
    public string? TenderRequestId { get; set; }
    public TenderRequest? TenderRequest { get; set; }
    public string SenderCompanyProfileId { get; set; } = "";
    public Company SenderCompany { get; set; } = null!;
    public string ReceiverCompanyProfileId { get; set; } = "";
    public Company ReceiverCompany { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class Message : IHasTimestamps
{
    public string Id { get; set; } = Ids.New();
    public string ChatRoomId { get; set; } = "";
    public ChatRoom ChatRoom { get; set; } = null!;
    public string Content { get; set; } = "";
    public string? LanguageCode { get; set; }
    public Language? Language { get; set; }
    public string? PdfUrl { get; set; }
    public bool ReadStatus { get; set; }
    public string SenderCompanyProfileId { get; set; } = "";
    public Company SenderCompany { get; set; } = null!;
    public string ReceiverCompanyProfileId { get; set; } = "";
    public Company ReceiverCompany { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CompanyRating : IHasCreatedAt
{
    public string Id { get; set; } = Ids.New();
    public string RatingCompanyId { get; set; } = "";
    public Company RatingCompany { get; set; } = null!;
    public string RatedCompanyId { get; set; } = "";
    public Company RatedCompany { get; set; } = null!;
    public string? TenderId { get; set; }
    public Tender? Tender { get; set; }
    public string TenderRequestId { get; set; } = "";
    public TenderRequest TenderRequest { get; set; } = null!;
    public int Quality { get; set; }
    public int Communication { get; set; }
    public int Experience { get; set; }
    public int Deadline { get; set; }
    public double OverallRating { get; set; }
    public string? Comment { get; set; }
    public string? LanguageCode { get; set; }
    public Language? Language { get; set; }
    public bool Anonymous { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public static double ComputeOverall(int quality, int communication, int experience, int deadline) =>
        Math.Round((quality + communication + experience + deadline) / 4d, 2);
}

public class Notification : IHasCreatedAt
{
    public string Id { get; set; } = Ids.New();
    public string UserId { get; set; } = "";
    public UserProfile User { get; set; } = null!;
    public string Type { get; set; } = "";
    public JsonDocument? Params { get; set; }
    public string? TenderId { get; set; }
    public Tender? Tender { get; set; }
    public string? ActorId { get; set; }
    public UserProfile? Actor { get; set; }
    public bool Read { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
