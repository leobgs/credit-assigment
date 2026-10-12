namespace PengajuanKredit.Api.Dtos;

public record LoginResponse(
    string Token,
    DateTime ExpiresAt
);
