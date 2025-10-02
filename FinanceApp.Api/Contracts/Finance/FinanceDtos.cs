namespace FinanceApp.Api.Contracts.Finance;

public class BalanceResponse
{
    public decimal TotalBalance { get; set; }
    public decimal ReserveBalance { get; set; }
    public decimal ReservePercentage { get; set; }
}

public class AddTransactionRequest
{
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // income | expense
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
}

public class TransactionResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
}

public class HistoryQuery
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Category { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Search { get; set; }
    public string? Period { get; set; } // week, month, 3m, 6m, year, all
}

// Chart/Report DTOs
public class BalanceReportData
{
    public string Date { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class ExpenseReportData
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class TransactionHistoryResponse
{
    public List<TransactionResponse> Transactions { get; set; } = new();
    public int TotalCount { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
}


