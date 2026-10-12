using PengajuanKredit.Api.Dtos;

namespace PengajuanKredit.Api.Services;

public interface IPengajuanKreditService
{
    Task<PengajuanKreditResponse> CreateAsync(CreatePengajuanKreditRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<PengajuanKreditResponse>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<PengajuanKreditResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PengajuanKreditResponse> UpdateAsync(Guid id, UpdatePengajuanKreditRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
