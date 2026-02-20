using System.ComponentModel.DataAnnotations;
using _2_service.Models;
using _3_dataaccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LoginRequest = _2_service.Models.LoginRequest;

namespace _2_service;

public interface IAuthService
{
    Task<AuthUserInfo> Login(LoginRequest request);
    AuthUserInfo GetUser(string id);
}


public class AuthService(MyDbContext ctx, IPasswordHasher<User> hasher) : IAuthService
{
    public async Task<AuthUserInfo> Login(LoginRequest request)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);

        try
        {
            var dbUser = ctx.Users.First(u => u.Username == request.Username);
            return AuthLogin(request, dbUser);
        }
        catch (InvalidOperationException _)
        {
            return await NewUser(request);
        }
    }

    private async Task<AuthUserInfo> NewUser(LoginRequest request)
    {
        var user = new User()
        {
            Username = request.Username,
            Createdat = DateTime.UtcNow,
            Id = Guid.NewGuid().ToString(),
        };
        user.Passwordhash = hasher.HashPassword(user, request.Password);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();
        return new AuthUserInfo(user.Id, user.Username, "User");
    }

    private AuthUserInfo AuthLogin(LoginRequest request, User foundUser)
    {
        return hasher.VerifyHashedPassword(foundUser, foundUser.Passwordhash, request.Password) !=
               PasswordVerificationResult.Success ? throw new ValidationException("Username or password is wrong you fucking idiot") : new AuthUserInfo(foundUser.Id, foundUser.Username, "User");
    }
    
    public AuthUserInfo GetUser(string userId)
    {
        var user = ctx.Users.First(u => u.Id == userId);
        return new AuthUserInfo(user.Id, user.Username, "User");
    }
}