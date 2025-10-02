namespace FinanceApp.Api.Contracts.Auth;

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Profession { get; set; }
    public decimal Salary { get; set; }
    public decimal AdditionalEarnings { get; set; }
    public string? Currency { get; set; }
    public int? SalaryDay { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}


