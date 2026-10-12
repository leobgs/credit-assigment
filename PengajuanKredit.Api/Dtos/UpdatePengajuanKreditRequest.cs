namespace PengajuanKredit.Api.Dtos;

public record UpdatePengajuanKreditRequest(
    decimal Plafon,
    decimal Bunga,
    int Tenor
);
