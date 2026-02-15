using Microsoft.AspNetCore.SignalR.Protocol;

namespace _2_service.Models;

public class ExtendedRoomResponse(string id, string chatName, List<MessageResponse> messages)
{
    public string Id { get; set; } = id;
    public string ChatName { get; set; } = chatName;
    public List<MessageResponse> Messages { get; set; } = messages;
}