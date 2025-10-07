namespace HiLoGame.Application.Common.Exceptions;

public sealed class ValidationException : AppException
{
    public ValidationException()
        : base("validation_error", "One or more validation errors occurred.") {}
}