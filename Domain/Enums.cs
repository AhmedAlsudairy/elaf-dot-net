using NpgsqlTypes;

namespace Munaqasat.Web.Domain;

/// <summary>Maps to the Postgres enum <c>text_direction</c>.</summary>
public enum TextDirection
{
    [PgName("ltr")] Ltr,
    [PgName("rtl")] Rtl
}

/// <summary>Maps to the Postgres enum <c>request_status</c>.</summary>
public enum RequestStatus
{
    [PgName("PENDING")] Pending,
    [PgName("ACCEPTED")] Accepted,
    [PgName("REJECTED")] Rejected,
    [PgName("WITHDRAWN")] Withdrawn
}

public static class ProcurementCategories
{
    public const string Supply = "SUPPLY";
    public const string Services = "SERVICES";
    public const string Works = "WORKS";

    public static readonly string[] All = { Supply, Services, Works };
}

/// <summary>Values for <c>tenders.status</c> (an addition to the ERD so the form's "Save as draft" works).</summary>
public static class TenderStatuses
{
    public const string Draft = "DRAFT";
    public const string Published = "PUBLISHED";
}

/// <summary>Values for <c>companies.verification_status</c> (an addition to the ERD for the admin console).</summary>
public static class VerificationStatuses
{
    public const string Pending = "PENDING";
    public const string Verified = "VERIFIED";
}

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Client = "Client";
    public const string Contractor = "Contractor";
    public const string Individual = "Individual";

    public static readonly string[] All = { Administrator, Client, Contractor, Individual };
}
