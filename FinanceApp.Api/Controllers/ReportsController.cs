using System.Security.Claims;
using FinanceApp.Api.Contracts.Finance;
using FinanceApp.Api.Domain.Entities;
using FinanceApp.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ReportsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("balance")]
        public async Task<ActionResult<List<BalanceReportData>>> GetBalanceReport([FromQuery] string period = "month")
        {
            var userId = GetUserId();
            var now = DateTime.UtcNow;
            var startDate = period.ToLowerInvariant() switch
            {
                "week" => now.AddDays(-7),
                "month" => now.AddMonths(-1),
                "3months" => now.AddMonths(-3),
                "6months" => now.AddMonths(-6),
                "year" => now.AddYears(-1),
                "all" => now.AddYears(-10), // Go back 10 years for "all"
                _ => now.AddMonths(-1)
            };

            // Try to get data from BalanceHistory first
            var historyData = await _db.BalanceHistory
                .AsNoTracking()
                .Where(b => b.UserId == userId && b.Date >= startDate)
                .OrderBy(b => b.Date)
                .Select(b => new BalanceReportData
                {
                    Date = b.Date.ToString("yyyy-MM-dd"),
                    Balance = b.Balance
                })
                .ToListAsync();

            // If no history data, generate from transactions
            if (!historyData.Any())
            {
                var transactions = await _db.Transactions
                    .AsNoTracking()
                    .Where(t => t.UserId == userId && t.Date >= startDate)
                    .OrderBy(t => t.Date)
                    .ToListAsync();

                var currentBalance = await _db.Balances
                    .AsNoTracking()
                    .Where(b => b.UserId == userId)
                    .Select(b => b.TotalBalance)
                    .FirstOrDefaultAsync();

                // Generate daily balance data
                var balanceData = new List<BalanceReportData>();
                var runningBalance = currentBalance;

                // Work backwards from current balance
                for (int i = transactions.Count - 1; i >= 0; i--)
                {
                    var transaction = transactions[i];
                    if (transaction.Type == TransactionType.Income)
                        runningBalance -= transaction.Amount;
                    else
                        runningBalance += transaction.Amount;

                    balanceData.Insert(0, new BalanceReportData
                    {
                        Date = transaction.Date.ToString("yyyy-MM-dd"),
                        Balance = runningBalance
                    });
                }

                // Add current balance
                balanceData.Add(new BalanceReportData
                {
                    Date = now.ToString("yyyy-MM-dd"),
                    Balance = currentBalance
                });

                historyData = balanceData;
            }

            return historyData;
        }

        [HttpGet("expenses")]
        public async Task<ActionResult<List<ExpenseReportData>>> GetExpenseReport([FromQuery] string period = "month")
        {
            var userId = GetUserId();
            var now = DateTime.UtcNow;
            var startDate = period.ToLowerInvariant() switch
            {
                "week" => now.AddDays(-7),
                "month" => now.AddMonths(-1),
                "3months" => now.AddMonths(-3),
                "6months" => now.AddMonths(-6),
                "year" => now.AddYears(-1),
                "all" => now.AddYears(-10),
                _ => now.AddMonths(-1)
            };

            var expenses = await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId && 
                           t.Type == TransactionType.Expense && 
                           t.Date >= startDate)
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            var totalExpenses = expenses.Sum(e => e.Amount);

            var result = expenses.Select(e => new ExpenseReportData
            {
                Category = e.Category,
                Amount = e.Amount,
                Percentage = totalExpenses > 0 ? Math.Round((e.Amount / totalExpenses) * 100, 1) : 0
            })
            .OrderByDescending(e => e.Amount)
            .ToList();

            return result;
        }

        private Guid GetUserId()
        {
            var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(sub!);
        }
    }
}
