namespace Munaqasat.Web.Domain.Entities;

public class Sector : BaseEntity
{
    public string Code { get; set; } = "";
    public string NameEn { get; set; } = "";
    public string NameAr { get; set; } = "";

    public ICollection<CompanySector> Companies { get; set; } = new List<CompanySector>();
}

public class Governorate : BaseEntity
{
    public string Code { get; set; } = "";
    public string NameEn { get; set; } = "";
    public string NameAr { get; set; } = "";

    public ICollection<Wilayat> Wilayats { get; set; } = new List<Wilayat>();
}

public class Wilayat : BaseEntity
{
    public Guid GovernorateId { get; set; }
    public Governorate Governorate { get; set; } = null!;

    public string NameEn { get; set; } = "";
    public string NameAr { get; set; } = "";
}
