using Munaqasat.Web.Domain;
using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Services.Common;

namespace Munaqasat.Web.Services.Companies;

public sealed class CompanyProfileInput
{
    public string Name { get; set; } = "";
    public string NameAr { get; set; } = "";
    public string? CommercialRegistrationNumber { get; set; }
    public string Address { get; set; } = "";
    public string? Website { get; set; }
    public string Bio { get; set; } = "";
    public string BioAr { get; set; } = "";
    public Guid? GovernorateId { get; set; }

    public List<Guid> SectorIds { get; set; } = new();
}

public interface ICompanyService
{
    Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAsync(CompanyProfileInput input, CancellationToken cancellationToken = default);

    Task<OperationResult> UpdateProfileAsync(Guid id, CompanyProfileInput input, CancellationToken cancellationToken = default);

    Task<OperationResult> SetVerificationStatusAsync(Guid id, CompanyVerificationStatus status, CancellationToken cancellationToken = default);

    Task RecalculateRatingAsync(Guid companyId, CancellationToken cancellationToken = default);
}
