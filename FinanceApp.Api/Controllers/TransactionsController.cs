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
    [Route("api")] // explicit routes per action
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public TransactionsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("balances")]
        public async Task<ActionResult<BalanceResponse>> GetBalances()
        {
            var userId = GetUserId();
            var balance = await _db.Balances.AsNoTracking().FirstOrDefaultAsync(b => b.UserId == userId);
            if (balance == null)
            {
                return new BalanceResponse { TotalBalance = 0, ReserveBalance = 0 };
            }

            return new BalanceResponse
            {
                TotalBalance = balance.TotalBalance,
                ReserveBalance = balance.ReserveBalance,
                ReservePercentage = balance.ReservePercentage
            };
        }

        [HttpPost("transactions")]
        public async Task<ActionResult<TransactionResponse>> AddTransaction([FromBody] AddTransactionRequest request)
        {
            var userId = GetUserId();
            var balance = await _db.Balances.FirstOrDefaultAsync(b => b.UserId == userId);
            if (balance == null)
            {
                balance = new Balance { UserId = userId, ReservePercentage = 10 };
                await _db.Balances.AddAsync(balance);
            }

            var isIncome = string.Equals(request.Type, "income", StringComparison.OrdinalIgnoreCase);
            var isExpense = string.Equals(request.Type, "expense", StringComparison.OrdinalIgnoreCase);
            if (!isIncome && !isExpense)
            {
                return BadRequest(new { message = "type must be 'income' or 'expense'" });
            }

            var tx = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = request.Amount,
                Type = isIncome ? TransactionType.Income : TransactionType.Expense,
                Category = request.Category,
                Description = request.Description,
                Date = request.Date ?? DateTime.UtcNow
            };

            if (isIncome)
            {
                balance.TotalBalance += request.Amount;
                var reserveAmount = Math.Round(request.Amount * balance.ReservePercentage / 100m, 2);
                balance.ReserveBalance += reserveAmount;
            }
            else
            {
                balance.TotalBalance -= request.Amount;
            }

            await _db.Transactions.AddAsync(tx);
            await _db.SaveChangesAsync();

            return new TransactionResponse
            {
                Id = tx.Id,
                Amount = tx.Amount,
                Type = tx.Type.ToString().ToLowerInvariant(),
                Category = tx.Category,
                Description = tx.Description,
                Date = tx.Date
            };
        }

        [HttpGet("transactions/history")]
        public async Task<ActionResult<TransactionHistoryResponse>> History([FromQuery] HistoryQuery query)
        {
            var userId = GetUserId();
            var q = _db.Transactions.AsNoTracking().Where(t => t.UserId == userId);

            if (!string.IsNullOrWhiteSpace(query.Period) && (query.From == null && query.To == null))
            {
                var now = DateTime.UtcNow;
                query.Period = query.Period.ToLowerInvariant();
                query.To = now;
                query.From = query.Period switch
                {
                    "week" => now.AddDays(-7),
                    "month" => now.AddMonths(-1),
                    "3m" => now.AddMonths(-3),
                    "6m" => now.AddMonths(-6),
                    "year" => now.AddYears(-1),
                    _ => null
                };
            }

            if (query.From.HasValue)
                q = q.Where(t => t.Date >= query.From.Value);
            if (query.To.HasValue)
                q = q.Where(t => t.Date <= query.To.Value);
            if (!string.IsNullOrWhiteSpace(query.Category))
                q = q.Where(t => t.Category == query.Category);
            if (query.MinAmount.HasValue)
                q = q.Where(t => t.Amount >= query.MinAmount.Value);
            if (query.MaxAmount.HasValue)
                q = q.Where(t => t.Amount <= query.MaxAmount.Value);
            if (!string.IsNullOrWhiteSpace(query.Search))
                q = q.Where(t => (t.Description ?? "").ToLower().Contains(query.Search.ToLower()) || t.Category.ToLower().Contains(query.Search.ToLower()));

            var items = await q.OrderByDescending(t => t.Date)
                .Select(t => new TransactionResponse
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Type = t.Type.ToString().ToLower(),
                    Category = t.Category,
                    Description = t.Description,
                    Date = t.Date
                })
                .ToListAsync();

            var totalIncome = items.Where(t => t.Type == "income").Sum(t => t.Amount);
            var totalExpenses = items.Where(t => t.Type == "expense").Sum(t => t.Amount);

            return new TransactionHistoryResponse
            {
                Transactions = items,
                TotalCount = items.Count,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses
            };
        }

        private Guid GetUserId()
        {
            var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(sub!);
        }
    }
}
