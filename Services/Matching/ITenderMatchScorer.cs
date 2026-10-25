namespace Munaqasat.Web.Services.Matching;

public sealed record TenderMatchScore(int Value, string Reason);

public sealed record CompanyMatchProfile(
    Guid CompanyId,
    IReadOnlyCollection<string> SectorNames,
    Guid? GovernorateId,
    string? GovernorateName);

public interface ITenderMatchScorer
{
    TenderMatchScore Score(TenderMatchCandidate tender, CompanyMatchProfile company);
}

public sealed record TenderMatchCandidate(
    Guid TenderId,
    string Title,
    string CategoryLabel,
    Guid? GovernorateId);
