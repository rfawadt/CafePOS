using CafePos.Application.Interfaces;
using CafePos.Application.Models;

namespace CafePos.Application.Services;

public class AuthService
{
    private readonly IPosDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthService(IPosDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(new LoginResult(false, "Username and password are required.", null, null, Array.Empty<string>()));
        }

        var user = _db.Users.FirstOrDefault(u => u.IsActive && u.Username.ToLowerInvariant() == username.ToLowerInvariant());
        if (user == null)
        {
            return Task.FromResult(new LoginResult(false, "Invalid credentials.", null, null, Array.Empty<string>()));
        }

        if (!_hasher.Verify(password, user.PasswordHash, user.PasswordSalt))
        {
            return Task.FromResult(new LoginResult(false, "Invalid credentials.", null, null, Array.Empty<string>()));
        }

        var roles = (from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.RoleId
            where ur.UserId == user.UserId
            select r.Name).ToList();

        return Task.FromResult(new LoginResult(true, "OK", user.UserId, user.DisplayName, roles));
    }
}
