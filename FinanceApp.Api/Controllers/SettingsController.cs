using System.Security.Claims;
using FinanceApp.Api.Contracts.Settings;
using FinanceApp.Api.Domain.Entities;
using FinanceApp.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private static readonly int[] AllowedPercentages = new[] { 1, 3, 5, 10, 15, 20, 25, 30 };

        private readonly ApplicationDbContext _db;

        public SettingsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPut("reserve")]
        public async Task<IActionResult> UpdateReserve([FromBody] UpdateReservePercentageRequest request)
        {
            if (!AllowedPercentages.Contains(request.ReservePercentage))
            {
                return BadRequest(new { message = "percentage must be one of 1,3,5,10,15,20,25,30" });
            }

            var userId = GetUserId();
            var balance = await _db.Balances.FirstOrDefaultAsync(b => b.UserId == userId);
            if (balance == null)
            {
                balance = new Balance { UserId = userId };
                await _db.Balances.AddAsync(balance);
            }

            balance.ReservePercentage = request.ReservePercentage;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("needs")]
        public async Task<ActionResult<Contracts.Settings.NonExcludableNeed>> AddNeed([FromBody] AddNonExcludableNeedRequest request)
        {
            var userId = GetUserId();
            var need = new Domain.Entities.NonExcludableNeed
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = request.Category,
                Amount = request.Amount
            };
            await _db.NonExcludableNeeds.AddAsync(need);
            await _db.SaveChangesAsync();
            
            return new Contracts.Settings.NonExcludableNeed
            {
                Id = need.Id,
                Category = need.Category,
                Amount = need.Amount
            };
        }

        [HttpPost("goals")]
        public async Task<ActionResult<Contracts.Settings.SavingsGoal>> AddGoal([FromBody] CreateSavingsGoalRequest request)
        {
            var userId = GetUserId();
            var goal = new Domain.Entities.SavingsGoal
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GoalName = request.GoalName,
                TargetAmount = request.TargetAmount,
                CurrentAmount = 0,
                Progress = 0
            };
            await _db.SavingsGoals.AddAsync(goal);
            await _db.SaveChangesAsync();
            
            return new Contracts.Settings.SavingsGoal
            {
                Id = goal.Id,
                GoalName = goal.GoalName,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                Progress = goal.Progress
            };
        }

        [HttpGet]
        public async Task<ActionResult<SettingsResponse>> GetSettings()
        {
            var userId = GetUserId();
            var balance = await _db.Balances.AsNoTracking().FirstOrDefaultAsync(b => b.UserId == userId);
            var needs = await _db.NonExcludableNeeds.AsNoTracking().Where(n => n.UserId == userId)
                .Select(n => new Contracts.Settings.NonExcludableNeed { Id = n.Id, Category = n.Category, Amount = n.Amount })
                .ToListAsync();
            var goals = await _db.SavingsGoals.AsNoTracking().Where(g => g.UserId == userId)
                .Select(g => new Contracts.Settings.SavingsGoal { Id = g.Id, GoalName = g.GoalName, TargetAmount = g.TargetAmount, CurrentAmount = g.CurrentAmount, Progress = g.Progress })
                .ToListAsync();

            return new SettingsResponse
            {
                ReservePercentage = balance?.ReservePercentage ?? 0,
                NonExcludableNeeds = needs,
                SavingsGoals = goals
            };
        }

        private Guid GetUserId()
        {
            var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(sub!);
        }
    }
}
