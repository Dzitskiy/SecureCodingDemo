using System.Security.Cryptography;

namespace SecureCodingDemo.Services;

public sealed class DemoRepository
{
    private readonly List<DemoUser> _users =
    [
        new(1, "alice", "Alice", "Engineering", false, HashPassword("correct-horse-battery-staple")),
        new(2, "bob", "Bob", "Finance", false, HashPassword("pa55word")),
        new(3, "admin", "Ada Admin", "Security", true, HashPassword("admin-demo-password"))
    ];

    private readonly List<DemoOrder> _orders =
    [
        new(101, 1, "Alice laptop order", 1800m),
        new(102, 1, "Alice cloud credits", 250m),
        new(103, 2, "Bob payroll export", 900m)
    ];

    public UnsafeLoginResult UnsafeLogin(string userName, string password)
    {
        var sql = $"select * from users where username = '{userName}' and password = '{password}'";
        var injectionDetected = userName.Contains("' OR '1'='1", StringComparison.OrdinalIgnoreCase)
            || password.Contains("' OR '1'='1", StringComparison.OrdinalIgnoreCase);

        var user = injectionDetected
            ? _users.First()
            : _users.FirstOrDefault(item => item.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                && item.PasswordHash == HashPassword(password));

        return new UnsafeLoginResult(sql, injectionDetected, user);
    }

    public DemoUser? SafeLogin(string userName, string password)
    {
        return _users.FirstOrDefault(item => item.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
            && item.PasswordHash == HashPassword(password));
    }

    public DemoUser? GetUser(int userId)
    {
        return _users.FirstOrDefault(item => item.Id == userId);
    }

    public DemoUser UpdateUnsafeProfile(UnsafeProfileUpdate update)
    {
        var user = _users.First();
        user.DisplayName = update.DisplayName ?? user.DisplayName;
        user.Department = update.Department ?? user.Department;
        user.IsAdmin = update.IsAdmin ?? user.IsAdmin;
        return user;
    }

    public DemoUser UpdateSafeProfile(SafeProfileUpdate update)
    {
        var user = _users.First();
        user.DisplayName = update.DisplayName;
        user.Department = update.Department;
        return user;
    }

    public DemoOrder? GetOrderUnsafe(int orderId)
    {
        return _orders.FirstOrDefault(item => item.Id == orderId);
    }

    public DemoOrder? GetOrderForUser(int orderId, int userId)
    {
        return _orders.FirstOrDefault(item => item.Id == orderId && item.UserId == userId);
    }

    private static string HashPassword(string password)
    {
        return Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)));
    }
}

public sealed class DemoUser
{
    public DemoUser(int id, string userName, string displayName, string department, bool isAdmin, string passwordHash)
    {
        Id = id;
        UserName = userName;
        DisplayName = displayName;
        Department = department;
        IsAdmin = isAdmin;
        PasswordHash = passwordHash;
    }

    public int Id { get; }

    public string UserName { get; }

    public string DisplayName { get; set; }

    public string Department { get; set; }

    public bool IsAdmin { get; set; }

    public string PasswordHash { get; }
}

public sealed record DemoOrder(int Id, int UserId, string Description, decimal Amount);

public sealed record UnsafeLoginResult(string Sql, bool InjectionDetected, DemoUser? User);

public sealed record UnsafeProfileUpdate(string? DisplayName, string? Department, bool? IsAdmin);

public sealed record SafeProfileUpdate(string DisplayName, string Department);
