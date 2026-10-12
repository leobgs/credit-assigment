namespace PengajuanKredit.Api.Dtos;

public record PengajuanKreditResponse(
    Guid Id,
    decimal Plafon,
    decimal Bunga,
    int Tenor,
    decimal Angsuran,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
