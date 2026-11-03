namespace Munaqasat.Web.Domain.Entities;

/// <summary>Rows with a <c>created_at</c> column, stamped by the DbContext.</summary>
public interface IHasCreatedAt
{
    DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Rows with <c>created_at</c> and <c>updated_at</c>, stamped by the DbContext and a DB trigger.</summary>
public interface IHasTimestamps : IHasCreatedAt
{
    DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>A translation row: one per (record, language).</summary>
public interface ITranslation
{
    string LanguageCode { get; }
}

public static class Ids
{
    public static string New() => Guid.NewGuid().ToString("N");
}
