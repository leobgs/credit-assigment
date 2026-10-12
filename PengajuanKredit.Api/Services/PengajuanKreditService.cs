using Microsoft.EntityFrameworkCore;
using PengajuanKredit.Api.Data;
using PengajuanKredit.Api.Dtos;
using PengajuanKredit.Api.Exceptions;
using PengajuanKredit.Api.Models;

namespace PengajuanKredit.Api.Services;

public class PengajuanKreditService : IPengajuanKreditService
{
    private readonly AppDbContext _context;
    private readonly IAngsuranCalculator _calculator;

    public PengajuanKreditService(AppDbContext context, IAngsuranCalculator calculator)
    {
        _context = context;
        _calculator = calculator;
    }

    public async Task<PengajuanKreditResponse> CreateAsync(CreatePengajuanKreditRequest request, CancellationToken cancellationToken = default)
    {
        decimal angsuran = _calculator.HitungAngsuranPerBulan(request.Plafon, request.Bunga, request.Tenor);
        var now = DateTime.UtcNow;

        var entity = new Models.PengajuanKredit
        {
            Id = Guid.NewGuid(),
            Plafon = request.Plafon,
            Bunga = request.Bunga,
            Tenor = request.Tenor,
            Angsuran = angsuran,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.PengajuanKredits.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(entity);
    }

    public async Task<PagedResult<PengajuanKreditResponse>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        int totalCount = await _context.PengajuanKredits.CountAsync(cancellationToken);

        var items = await _context.PengajuanKredits
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapToResponse(x))
            .ToListAsync(cancellationToken);

        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<PengajuanKreditResponse>(
            items,
            page,
            pageSize,
            totalCount,
            totalPages
        );
    }

    public async Task<PengajuanKreditResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PengajuanKredits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Models.PengajuanKredit), id);
        }

        return MapToResponse(entity);
    }

    public async Task<PengajuanKreditResponse> UpdateAsync(Guid id, UpdatePengajuanKreditRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PengajuanKredits
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Models.PengajuanKredit), id);
        }

        decimal angsuran = _calculator.HitungAngsuranPerBulan(request.Plafon, request.Bunga, request.Tenor);

        entity.Plafon = request.Plafon;
        entity.Bunga = request.Bunga;
        entity.Tenor = request.Tenor;
        entity.Angsuran = angsuran;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PengajuanKredits
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Models.PengajuanKredit), id);
        }

        _context.PengajuanKredits.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static PengajuanKreditResponse MapToResponse(Models.PengajuanKredit entity)
    {
        return new PengajuanKreditResponse(
            entity.Id,
            entity.Plafon,
            entity.Bunga,
            entity.Tenor,
            entity.Angsuran,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
