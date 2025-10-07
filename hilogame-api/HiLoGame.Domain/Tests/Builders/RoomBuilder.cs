
using HiLoGame.Domain.Aggregates.Room.Entities;

namespace HiLoGame.Domain.Tests.Builders;

public sealed class RoomBuilder
{
    private string _name = "TestRoom";
    private string _ownerId = "owner";
    private string _ownerUsername = "owner";


    public RoomBuilder WithOwner(string id, string username)
    {
        _ownerId = id; _ownerUsername = username;
        return this;
    }

    public RoomBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Room Build()
    {
        var room = new Room(_name, _ownerId, _ownerUsername);
        return room;
    }
}