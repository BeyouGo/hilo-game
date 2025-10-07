namespace HiLoGame.Application.Common.Exceptions;

public sealed class NotFoundException : AppException
{
    private readonly IReadOnlyDictionary<string, object?> _meta;

    public NotFoundException(string entity, object key)
        : base("not_found", $"{entity} with key '{key}' was not found")
        => _meta = new Dictionary<string, object?> { ["entity"] = entity, ["key"] = key };
}