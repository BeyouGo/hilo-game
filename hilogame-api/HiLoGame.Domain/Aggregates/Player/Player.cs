using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace HiLoGame.Domain.Aggregates.Player;

public class Player : IdentityUser
{
    //public string Id { get; set; }
    //public string Username { get; set; }
    public DateTime RegisteredAt { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }


    private static readonly Regex UsernameRegex = new(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

    private Player() {} // EF

    public Player(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty.", nameof(username));
        }

        if (username.Length > 64)
        {

            throw new ArgumentException("Username couldn't be longer than 25 characters.", nameof(username));
        }

        if (!UsernameRegex.IsMatch(username))
        {
            throw new ArgumentException("Username can only contain letters, digits or underscore.", nameof(username));
        }

        base.UserName = username;
        RegisteredAt = DateTime.Now;

    }
}