using FinanceApp.Api.Infrastructure.Data;
using FinanceApp.Api.Application.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Api.Application.Jobs;

public interface INeedsDeductionJob
{
    Task RunAsync(CancellationToken cancellationToken = default);
}

public class NeedsDeductionJob : INeedsDeductionJob
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;

    public NeedsDeductionJob(ApplicationDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

        var users = await _db.Users.Include(u => u.Balance)
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            var needs = await _db.NonExcludableNeeds.Where(n => n.UserId == user.Id).ToListAsync(cancellationToken);
            if (needs.Count == 0) continue;

            var totalNeeds = needs.Sum(n => n.Amount);
            var daily = Math.Round(totalNeeds / daysInMonth, 2);

            user.Balance ??= new Domain.Entities.Balance { UserId = user.Id };
            user.Balance.TotalBalance -= daily;

            _db.Transactions.Add(new Domain.Entities.Transaction
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Amount = daily,
                Type = Domain.Entities.TransactionType.Expense,
                Category = "Needs",
                Description = "Daily needs deduction",
                Date = now
            });

            // Send daily expense deduction notification
            await _notificationService.SendDailyExpenseDeductionNotificationAsync(
                user.Id,
                "Daily Needs",
                daily,
                user.Currency ?? "USD"
            );
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}


