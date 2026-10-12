namespace PengajuanKredit.Api.Dtos;

public record HitungAngsuranRequest(
    decimal Plafon,
    decimal Bunga,
    int Tenor
);
