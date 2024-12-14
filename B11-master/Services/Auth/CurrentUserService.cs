using System;
using System.Security.Claims;
using Baigiamasis.Services.Auth.Interfaces;
using Microsoft.AspNetCore.Http;



public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipal _user;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _user = _httpContextAccessor.HttpContext?.User 
            ?? throw new UnauthorizedAccessException("No user context available");
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user ID");
            }
            return userId;
        }
    }

    public string Username
    {
        get
        {
            var username = _user.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                throw new UnauthorizedAccessException("Username not found in token");
            }
            return username;
        }
    }

    public IEnumerable<string> Roles
    {
        get
        {
            return _user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
        }
    }

    public bool IsAdmin => Roles.Contains("Admin");

    public bool CanAccessUser(Guid userId)
    {
        try
        {
            return UserId == userId || IsAdmin;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
} 