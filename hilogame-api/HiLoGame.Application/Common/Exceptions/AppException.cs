namespace HiLoGame.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string code, string message, Exception? inner = null)
        : base(message, inner) => Code = code;
    public string Code { get; }
}
