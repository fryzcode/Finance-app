using FinanceApp.Api.Infrastructure.Data;
using FinanceApp.Api.Application.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Api.Application.Jobs;

public interface ISalaryCreditJob
{
    Task RunAsync(CancellationToken cancellationToken = default);
}

public class SalaryCreditJob : ISalaryCreditJob
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;

    public SalaryCreditJob(ApplicationDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Day;
        var users = await _db.Users
            .Include(u => u.Balance)
            .Where(u => u.Salary > 0 && u.SalaryDay.HasValue && u.SalaryDay.Value == today)
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            user.Balance ??= new Domain.Entities.Balance { UserId = user.Id };
            user.Balance.TotalBalance += user.Salary;

            var reservePercent = Math.Clamp(user.Balance.ReservePercentage, 0, 100);
            var reserveAmount = Math.Round(user.Salary * reservePercent / 100m, 2);
            user.Balance.ReserveBalance += reserveAmount;

            _db.Transactions.Add(new Domain.Entities.Transaction
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Amount = user.Salary,
                Type = Domain.Entities.TransactionType.Income,
                Category = "Salary",
                Description = "Monthly salary credit",
                Date = DateTime.UtcNow
            });

            // Send salary added notification
            await _notificationService.SendSalaryAddedNotificationAsync(
                user.Id, 
                user.Salary, 
                user.Currency ?? "USD"
            );
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}


