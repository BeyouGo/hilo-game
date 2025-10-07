namespace HiLoGame.Application.Common.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Forbidden") : base("forbidden", message) { }
}