namespace FinanceApp.Api.Contracts.Profile;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Profession { get; set; }
    public decimal Salary { get; set; }
    public decimal AdditionalEarnings { get; set; }
    public string? Currency { get; set; }
    public int? SalaryDay { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? AvatarUrl { get; set; }
}

public class UpdateProfileRequest
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Profession { get; set; }
    public decimal? Salary { get; set; }
    public decimal? AdditionalEarnings { get; set; }
    public string? Currency { get; set; }
    public int? SalaryDay { get; set; }
    public string? AvatarUrl { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}


