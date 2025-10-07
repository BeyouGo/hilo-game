namespace HiLoGame.Application.Common.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(string message, IDictionary<string, object?>? meta = null)
        : base("conflict", message) => _meta = meta;
    private readonly IDictionary<string, object?>? _meta;
}