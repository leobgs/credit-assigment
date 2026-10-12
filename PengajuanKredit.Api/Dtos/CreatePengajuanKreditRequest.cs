namespace PengajuanKredit.Api.Dtos;

public record CreatePengajuanKreditRequest(
    decimal Plafon,
    decimal Bunga,
    int Tenor
);
