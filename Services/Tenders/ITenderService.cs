using Munaqasat.Web.Domain.Entities;
using Munaqasat.Web.Services.Common;

namespace Munaqasat.Web.Services.Tenders;

public interface ITenderService
{
    Task<PagedResult<TenderListItem>> SearchAsync(TenderQuery query, CancellationToken cancellationToken = default);

    Task<Tender?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAsync(TenderInput input, bool publishImmediately, CancellationToken cancellationToken = default);

    Task<OperationResult> UpdateAsync(Guid id, TenderInput input, CancellationToken cancellationToken = default);

    Task<OperationResult> PublishAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> CloseAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
