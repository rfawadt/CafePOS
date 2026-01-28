using CafePos.Application.Interfaces;
using CafePos.Domain.Entities;

namespace CafePos.Application.Services;

public class UserService
{
    private readonly IPosDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;

    public UserService(IPosDbContext db, IPasswordHasher hasher, IClock clock)
    {
        _db = db;
        _hasher = hasher;
        _clock = clock;
    }

    public async Task<User> CreateUserAsync(string username, string displayName, string password, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        if (_db.Users.Any(u => u.Username.ToLowerInvariant() == username.ToLowerInvariant()))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var (hash, salt) = _hasher.HashPassword(password);
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            DisplayName = displayName,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAtUtc = _clock.UtcNow,
            IsActive = true
        };

        await _db.AddAsync(user, cancellationToken);

        foreach (var roleName in roles)
        {
            var role = _db.Roles.FirstOrDefault(r => r.Name == roleName) ?? await CreateRoleAsync(roleName, cancellationToken);
            await _db.AddAsync(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId
            }, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<Role> CreateRoleAsync(string name, CancellationToken cancellationToken = default)
    {
        var role = new Role
        {
            RoleId = Guid.NewGuid(),
            Name = name
        };
        await _db.AddAsync(role, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return role;
    }
}
