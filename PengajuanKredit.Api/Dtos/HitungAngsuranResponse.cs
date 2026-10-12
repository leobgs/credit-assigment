namespace PengajuanKredit.Api.Dtos;

public record HitungAngsuranResponse(
    decimal Plafon,
    decimal Bunga,
    int Tenor,
    decimal AngsuranPerBulan
);
