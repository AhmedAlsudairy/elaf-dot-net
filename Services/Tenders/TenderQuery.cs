using Munaqasat.Web.Domain;

namespace Munaqasat.Web.Services.Tenders;

public enum TenderSort
{
    MatchScore = 0,
    ClosingSoonest = 1,
    NewestFirst = 2,
    BudgetHighest = 3
}

public sealed class TenderQuery
{
    public IReadOnlyCollection<TenderCategory>? Categories { get; init; }

    public Guid? GovernorateId { get; init; }

    public decimal? BudgetMin { get; init; }
    public decimal? BudgetMax { get; init; }

    public int? MinMatchScore { get; init; }

    public string? SearchTerm { get; init; }

    public TenderStatus? Status { get; init; } = TenderStatus.Published;

    public bool OnlyOpen { get; init; } = true;

    public Guid? ForCompanyId { get; init; }

    public TenderSort Sort { get; init; } = TenderSort.MatchScore;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
