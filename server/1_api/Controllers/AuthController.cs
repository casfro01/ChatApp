using _2_service;
using _2_service.Models;
using _2_service.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StateleSSE.AspNetCore;

namespace _1_api.Controllers;

[ApiController]
public class AuthController(IAuthService auth, ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<LoginResponseDto> Login([FromBody] LoginRequest request)
    {
        var res = await auth.Login(request);
        try
        {
            var token = tokenService.CreateToken(res);
            return new LoginResponseDto(token);
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    
}

public record LoginResponseDto(string Jwt) :  BaseResponseDto;