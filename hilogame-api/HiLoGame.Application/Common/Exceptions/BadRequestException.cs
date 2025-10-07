namespace HiLoGame.Application.Common.Exceptions;

public sealed class BadRequestException : AppException
{
    public BadRequestException(string message = "Bad request") : base("bad_request", message) { }
}