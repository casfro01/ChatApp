using System.ComponentModel.DataAnnotations;
using _2_service.Models;
using _3_dataaccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace _2_service;

public interface IAuthService
{
    Task<AuthUserInfo> Login(LoginRequest request);
}


public class AuthService(MyDbContext ctx, IPasswordHasher<User> hasher) : IAuthService
{
    public async Task<AuthUserInfo> Login(LoginRequest request)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);

        var dbUser = ctx.Users.FirstOrDefault(u => u.Username == request.Username, null);
        if (dbUser == null) return await NewUser(request);
        return AuthLogin(request, dbUser);
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
}