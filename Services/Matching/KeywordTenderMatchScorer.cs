namespace Munaqasat.Web.Services.Matching;

public sealed class KeywordTenderMatchScorer : ITenderMatchScorer
{
    private const int BaseScore = 20;
    private const int CategoryMatchPoints = 45;
    private const int TitleMatchPoints = 20;
    private const int SameGovernoratePoints = 15;

    public TenderMatchScore Score(TenderMatchCandidate tender, CompanyMatchProfile company)
    {
        var score = BaseScore;
        var reasons = new List<string>();

        var matchedSector = company.SectorNames
            .FirstOrDefault(sector => Contains(tender.CategoryLabel, sector));

        if (matchedSector is not null)
        {
            score += CategoryMatchPoints;
            reasons.Add($"{matchedSector} sector");
        }
        else
        {
            matchedSector = company.SectorNames.FirstOrDefault(sector => Contains(tender.Title, sector));

            if (matchedSector is not null)
            {
                score += TitleMatchPoints;
                reasons.Add($"{matchedSector} work");
            }
        }

        if (company.GovernorateId is { } companyGovernorate && tender.GovernorateId == companyGovernorate)
        {
            score += SameGovernoratePoints;
            if (company.GovernorateName is { Length: > 0 } name) reasons.Add($"based in {name}");
        }

        var reason = reasons.Count == 0
            ? "No sector overlap"
            : $"Matches your {string.Join(" and ", reasons)}";

        return new TenderMatchScore(Math.Clamp(score, 0, 100), reason);
    }

    private static bool Contains(string haystack, string needle) =>
        needle.Length > 0 && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
