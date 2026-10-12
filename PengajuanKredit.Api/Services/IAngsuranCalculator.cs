namespace PengajuanKredit.Api.Services;

public interface IAngsuranCalculator
{
    /// <summary>
    /// Menghitung angsuran per bulan menggunakan rumus anuitas:
    /// A = P * r / (1 - (1 + r)^-n)
    /// </summary>
    /// <param name="plafon">Plafon / Pokok pinjaman (P)</param>
    /// <param name="bungaPersenTahunan">Suku bunga tahunan dalam persen (misal: 12 untuk 12%)</param>
    /// <param name="tenorBulan">Jangka waktu pinjaman dalam bulan (n)</param>
    /// <returns>Besaran angsuran per bulan (dibulatkan 2 desimal)</returns>
    decimal HitungAngsuranPerBulan(decimal plafon, decimal bungaPersenTahunan, int tenorBulan);
}
