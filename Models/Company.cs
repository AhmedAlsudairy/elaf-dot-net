namespace Munaqasat.Web.Models;

public class CustomSection
{
    public string Tab { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

public class Review
{
    public string By { get; set; } = "";
    public DateOnly Date { get; set; }
    public int Quality { get; set; }
    public int Communication { get; set; }
    public int Experience { get; set; }
    public int Deadline { get; set; }
    public string Comment { get; set; } = "";
    public bool Anonymous { get; set; }
}

public class Company
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Initials { get; set; } = "";
    public string Address { get; set; } = "";
    public string Website { get; set; } = "";
    public List<string> Sectors { get; set; } = new();
    public double Overall { get; set; }
    public int RatingCount { get; set; }
    public Dictionary<string, double> Bars { get; set; } = new();
    public string Bio { get; set; } = "";
    public List<CustomSection> CustomSections { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
}

public class CompanyRow
{
    public string Name { get; set; } = "";
    public string Sectors { get; set; } = "";
    public double Rating { get; set; }
    public int Count { get; set; }
    public string Status { get; set; } = "";
}

public class UserRow
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string Company { get; set; } = "";
    public DateOnly Joined { get; set; }
}

public class TenderRequestRow
{
    public string TenderId { get; set; } = "";
    public string Company { get; set; } = "";
    public decimal Price { get; set; }
    public int Days { get; set; }
    public string Status { get; set; } = "";
    public DateOnly Date { get; set; }

    public string StatusClass => Status switch
    {
        "ACCEPTED" => "status-accepted",
        "REJECTED" => "status-rejected",
        "WITHDRAWN" => "status-withdrawn",
        _ => "status-pending"
    };
}

public class Tier
{
    public string Name { get; set; } = "";
    public string Price { get; set; } = "";
    public string Desc { get; set; } = "";
    public List<string> Features { get; set; } = new();
    public string Cta { get; set; } = "";
    public bool Highlight { get; set; }
}
