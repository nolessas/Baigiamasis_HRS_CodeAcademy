namespace Baigiamasis.Services.Auth.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        bool IsAdmin { get; }
        bool CanAccessUser(Guid userId);
        string Username { get; }
        IEnumerable<string> Roles { get; }
    }
}
