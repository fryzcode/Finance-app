namespace FinanceApp.Api.Contracts.Settings;

public class UpdateReservePercentageRequest
{
    public int ReservePercentage { get; set; }
}

public class AddNonExcludableNeedRequest
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CreateSavingsGoalRequest
{
    public string GoalName { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
}

public class SettingsResponse
{
    public int ReservePercentage { get; set; }
    public IEnumerable<NonExcludableNeed> NonExcludableNeeds { get; set; } = Enumerable.Empty<NonExcludableNeed>();
    public IEnumerable<SavingsGoal> SavingsGoals { get; set; } = Enumerable.Empty<SavingsGoal>();
}

public class NonExcludableNeed
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class SavingsGoal
{
    public Guid Id { get; set; }
    public string GoalName { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal Progress { get; set; }
}


