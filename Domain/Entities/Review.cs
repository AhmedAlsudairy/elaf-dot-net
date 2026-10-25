namespace Munaqasat.Web.Domain.Entities;

public class Review : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid AuthorUserId { get; set; }
    public AppUser AuthorUser { get; set; } = null!;

    public Guid? TenderId { get; set; }
    public Tender? Tender { get; set; }

    public int QualityRating { get; set; }
    public int CommunicationRating { get; set; }
    public int ExperienceRating { get; set; }
    public int DeadlineRating { get; set; }

    public string Comment { get; set; } = "";

    public bool IsAnonymous { get; set; }

    public bool IsPublished { get; set; } = true;

    public double Overall =>
        (QualityRating + CommunicationRating + ExperienceRating + DeadlineRating) / 4d;
}
