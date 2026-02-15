using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using _2_service;
using _2_service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StateleSSE.AspNetCore;

namespace _1_api.Controllers;

[ApiController]
[Authorize]
public class ChatController(ISseBackplane backplane, IAuthService userService, IGroupService roomService) : ControllerBase
{
    [HttpGet(nameof(Connect))]
    [Produces<ConnectionResponse>]
    public async Task Connect()
    {
        await using var sse = await HttpContext.OpenSseStreamAsync();
        await using var connection = backplane.CreateConnection();

        await sse.WriteAsync(nameof(ConnectionResponse), JsonSerializer.Serialize(new ConnectionResponse(connection.ConnectionId), new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
        
        await foreach (var evt in connection.ReadAllAsync(HttpContext.RequestAborted))
        {
            if (evt.Group != null)
                await sse.WriteAsync(evt.Group, evt.Data);
            else
                await sse.WriteAsync(evt.Data);
        }
    }
    
    
    [HttpPost(nameof(JoinGroup))]
    [ProducesResponseType(typeof(JoinGroupBroadcast), 202)]
    [ProducesResponseType(typeof(JoinGroupResponse), 200)]
    [ProducesResponseType(typeof(UserLeftResponseDto), 400)]
    public async Task<JoinGroupResponse> JoinGroup([FromBody] JoinGroupRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var u = userService.GetUser(userId);
        var room = roomService.GetRoom(request.Group);
        var name = u?.Username ?? "Anonymous";
        await backplane.Groups.AddToGroupAsync("nickname/"+request.ConnectionId, name);
        await backplane.Groups.AddToGroupAsync(request.ConnectionId, request.Group);
        var members = await backplane.Groups.GetMembersAsync(request.Group);
        var list = new List<ConnectionIdAndUserName>();
        foreach (var m in members)
        {
            var nickname = await backplane.Groups.GetClientGroupsAsync("nickname/" + m);
            list.Add(new ConnectionIdAndUserName(m, nickname.FirstOrDefault() ?? "Anonymous"));
        }
        await backplane.Clients.SendToGroupAsync(request.Group, new JoinGroupBroadcast(list));
        
        return new JoinGroupResponse(room);
    }
}

public record JoinGroupRequest(string ConnectionId, string Group);

public record UserLeftResponseDto : BaseResponseDto;


public record JoinGroupResponse(ExtendedRoomResponse room) : BaseResponseDto;

public record JoinGroupBroadcast(List<ConnectionIdAndUserName> ConnectedUsers) : BaseResponseDto;

public record ConnectionResponse(string ConnectionId) : BaseResponseDto;

public record ConnectionIdAndUserName(string ConnectionId, string UserName);