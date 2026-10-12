namespace PengajuanKredit.Api.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"Entitas '{entityName}' dengan id '{key}' tidak ditemukan.")
    {
    }
}
