using HiLoGame.Domain.Aggregates.Room.ValueObjects;
using System.Security.Cryptography;

namespace HiLoGame.Domain.Services;

public interface ISecretGenerator
{ 
    int Generate(GameRules rules);
}

public sealed class CryptoSecretGenerator : ISecretGenerator
{
    public int Generate(GameRules rule)
    {
        rule.EnsureValid();
        return RandomNumberGenerator.GetInt32(rule.Min, rule.Max + 1);
    }
}