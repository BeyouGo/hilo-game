using HiLoGame.Domain.Aggregates.Player;

namespace HiLoGame.Application.Models.Players;

public record PlayerResponse(string Id, string UserName)
{
    public string Id { get; set; } = Id;
    public string UserName { get; set; } = UserName;

    public static PlayerResponse From(Player player)
    {
        return new PlayerResponse(player.Id, player.UserName);
    }
}

// YAGNI
//public static class PlayerResponseExtensions
//{
//    public static readonly Expression<Func<Player, PlayerResponse>> ToPlayerResponseExpression = r => new PlayerResponse(r.Id, r.UserName);
//    public static readonly Func<Player, PlayerResponse> ToPlayerResponseFunc = ToPlayerResponseExpression.Compile();
//    public static IQueryable<PlayerResponse> SelectPlayerResponse(this IQueryable<Player> query)
//    {
//        return query.Select(ToPlayerResponseExpression);
//    }
//}