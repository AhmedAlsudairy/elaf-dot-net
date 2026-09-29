namespace Munaqasat.Web.Models;

public class CustomField
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

public class Tender
{
    public string Id { get; set; } = "";
    public string Entity { get; set; } = "";
    public string Title { get; set; } = "";
    public string TenderNo { get; set; } = "";
    public string ExtId { get; set; } = "";
    public string Category { get; set; } = "";
    public string CategoryLabel { get; set; } = "";
    public string Governorate { get; set; } = "";
    public string Wilayat { get; set; } = "";
    public string BudgetLabel { get; set; } = "";
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public DateOnly Published { get; set; }
    public DateOnly Closing { get; set; }
    public string DocFee { get; set; } = "";
    public string Bond { get; set; } = "";
    public int Score { get; set; }
    public bool IsNew { get; set; }
    public string Icon { get; set; } = "i-box";
    public string Description { get; set; } = "";
    public List<CustomField> CustomFields { get; set; } = new();

    public string ScoreTier => Score >= 90 ? "green" : (Score >= 75 ? "blue" : "orange");
    public string CategoryTagClass => Category switch
    {
        "SUPPLY" => "tag-blue",
        "SERVICES" => "tag-purple",
        _ => "tag-orange"
    };
    public string MatchLabel => Score >= 90 ? "Strong match" : (Score >= 75 ? "Good match" : "Partial match");
}
