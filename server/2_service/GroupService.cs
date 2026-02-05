using System.ComponentModel.DataAnnotations;
using _3_dataaccess;
using Microsoft.EntityFrameworkCore;

namespace _2_service;

public interface IGroupService
{
    List<RoomResponse> GetRooms();
    Task<RoomResponse> NewRoom(string newRoomName);
}

public class GroupService(MyDbContext ctx) : IGroupService
{
    public List<RoomResponse> GetRooms()
    {
        var list = ctx.Rooms.ToListAsync().Result;
        return list.Select(r => new RoomResponse(r.Id, r.Chatname)).ToList();
    }

    public async Task<RoomResponse> NewRoom(string newRoomName)
    {
        if (newRoomName.Length < 3) throw new ValidationException("Room name is too short");
        if (ctx.Rooms.Any(r => r.Chatname == newRoomName)) throw new ValidationException("Room already exists");
        var room = new Room()
        {
            Id = Guid.NewGuid().ToString(),
            Chatname = newRoomName
        };
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();
        return new RoomResponse(room.Id, room.Chatname);
    }
}