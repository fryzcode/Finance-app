using FinanceApp.Api.Application.Jobs;
using FinanceApp.Api.Application.Notifications;
using FinanceApp.Api.Domain.Entities;
using FinanceApp.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FinanceApp.Api.Tests.Jobs;

public class SalaryAndNeedsJobTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SalaryCreditJob_Adds_Salary_And_Reserve()
    {
        using var db = CreateDb();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "u@test.com",
            Name = "U",
            Surname = "T",
            PasswordHash = "x",
            Salary = 1000,
            SalaryDay = DateTime.UtcNow.Day,
            Balance = new Balance { UserId = Guid.Empty, ReservePercentage = 10 }
        };
        user.Balance.UserId = user.Id;
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var mockNotificationService = new Mock<INotificationService>();
        var job = new SalaryCreditJob(db, mockNotificationService.Object);
        await job.RunAsync();

        var reloaded = await db.Users.Include(u => u.Balance).FirstAsync();
        reloaded.Balance!.TotalBalance.Should().Be(1000);
        reloaded.Balance.ReserveBalance.Should().Be(100);
        (await db.Transactions.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task NeedsDeductionJob_Deducts_Daily_Total()
    {
        using var db = CreateDb();
        var user = new User
        {
            Id = Guid.NewGuid(), Email = "u@test.com", Name = "U", Surname = "T", PasswordHash = "x",
            Balance = new Balance { UserId = Guid.Empty, ReservePercentage = 10, TotalBalance = 200 }
        };
        user.Balance.UserId = user.Id;
        db.Users.Add(user);
        db.NonExcludableNeeds.AddRange(
            new NonExcludableNeed { Id = Guid.NewGuid(), UserId = user.Id, Category = "Rent", Amount = 300 },
            new NonExcludableNeed { Id = Guid.NewGuid(), UserId = user.Id, Category = "Utilities", Amount = 60 }
        );
        await db.SaveChangesAsync();

        var mockNotificationService = new Mock<INotificationService>();
        var job = new NeedsDeductionJob(db, mockNotificationService.Object);
        await job.RunAsync();

        var days = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
        var expectedDaily = Math.Round((300 + 60m) / days, 2);
        var reloaded = await db.Users.Include(u => u.Balance).FirstAsync();
        reloaded.Balance!.TotalBalance.Should().Be(200 - expectedDaily);
        (await db.Transactions.CountAsync()).Should().Be(1);
    }
}


