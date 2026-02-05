using _2_service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _1_api.Controllers;

[ApiController]
[Authorize]
public class GroupController(IGroupService service) : ControllerBase
{
    [HttpGet("Rooms")]
    public async Task<List<RoomResponse>> GetRooms()
    {
        return service.GetRooms();
    }

    [HttpPost("newRoom")]
    public async Task<RoomResponse> NewRoom([FromBody] string newRoomName)
    {
        return await service.NewRoom(newRoomName);
    }
}