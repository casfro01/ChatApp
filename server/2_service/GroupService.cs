using System.ComponentModel.DataAnnotations;
using _2_service.Models;
using _3_dataaccess;
using Microsoft.EntityFrameworkCore;

namespace _2_service;

public interface IGroupService
{
    List<RoomResponse> GetRooms();
    Task<RoomResponse> NewRoom(string newRoomName);
    ExtendedRoomResponse GetRoom(string id);

    Task<MessageResponse> NewMessage(string userId, string groupId, string content);
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
    
    public ExtendedRoomResponse GetRoom(string id)
    {
        var room = ctx.Rooms.Include(r => r.Messages).First(r => r.Id == id);
        List<MessageResponse> mes = new();
        room.Messages.ToList().ForEach(m => mes.Add(new MessageResponse(m.Chatmessage, m.Userid)));
        return new ExtendedRoomResponse(room.Id, room.Chatname, mes);
    }

    public async Task<MessageResponse> NewMessage(string userId, string groupId, string content)
    {
        var user = ctx.Users.First(u => u.Id == userId);
        var room = ctx.Rooms.First(g => g.Id == groupId);
        var mes = new Message()
        {
            Chatmessage = content,
            Room = room,
            Roomid = room.Id,
            User = user,
            Userid = user.Id,
            
        };
        
        room.Messages.Add(mes);
        await ctx.SaveChangesAsync();
        return new MessageResponse(mes.Chatmessage, user.Username);
    }
}