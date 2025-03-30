using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.API.DTO;
using FinanceTracker.API.Models;
using FinanceTracker.API.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


[ApiController]
[Route("api/[controller]")]
public class BudgetController : ControllerBase
{
    private readonly AppDbContext _context;
   

    public BudgetController(AppDbContext context)
    {
        _context = context;
        

    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateBudget([FromBody] BudgetDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var budget = new Budget
        {
            userId = userId,
            Income = request.Income,
        };
        _context.Budget.Add(budget);
        await _context.SaveChangesAsync();

        foreach (var category in request.Categories)
        {
            var newCategory = new Categories
            {
                UserId = userId,
                CategoryId = category.CategoryId,
                Amount = category.Amount,
                Category = category.Category,
                Color = category.Color
            };
            _context.Categories.Add(newCategory);
        }


        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        user.HasBudget = true;
        await _context.SaveChangesAsync();
        return Ok("Budget created successfully.");
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateBudget(UpdateBudgetRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var budget = await _context.Budget.FirstOrDefaultAsync(b => b.userId == userId);
        if (budget == null)
        {
            return NotFound("Budget not found.");
        }

        budget.Income = request.Income;

        if (request.DeletedAllocationIds != null && request.DeletedAllocationIds.Any())
        {
            foreach (var categoryId in request.DeletedAllocationIds)
            {
                var categoryToDelete = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

                if (categoryToDelete != null)
                {
                    _context.Categories.Remove(categoryToDelete);
                }
            }
        }

        foreach (var category in request.Categories)
        {
            if (category.Id < 0)
            {
                var newCategory = new Categories
                {
                    UserId = userId,
                    CategoryId = category.CategoryId,
                    Amount = category.Amount,
                    Category = category.Category,
                    Color = category.Color
                };
                _context.Categories.Add(newCategory);
            }
            else
            {
                var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);
                if (existingCategory == null)
                {
                    return NotFound("Category not found.");
                }
                existingCategory.Amount = category.Amount;
                existingCategory.Category = category.Category;
                existingCategory.Color = category.Color;
                existingCategory.CategoryId = category.CategoryId;
            }
        }





        await _context.SaveChangesAsync();
        return Ok("Budget updated successfully.");
    }

    [HttpGet("getBudget")]
    public async Task<IActionResult> GetBudget()
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userClaim == null)
        {
            return Unauthorized("User is not authenticated.");
        }

        if (!int.TryParse(userClaim.Value, out var userId))
        {
            return BadRequest("Invalid user ID.");
        }

        var budget = await _context.Budget.FirstOrDefaultAsync(b => b.userId == userId);

        if (budget == null)
        {
            return NotFound("Budget not found.");
        }
        var income = budget.Income;
        var categories = await _context.Categories.Where(c => c.UserId == userId).ToListAsync();

        var response = new 
        {
            Income = income,
            Categories = categories.Select(c => new CategoryRequest
            {
                Id = c.Id,
                CategoryId = c.CategoryId,
                Amount = c.Amount,
                Category = c.Category,
                Color = c.Color
            }).ToList()
        };
        return Ok(response);
    }



}
public class CreateCategoryRequest
{
    public string Category { get; set; }
    public int CategoryId { get; set; }
    public string Color { get; set; }
    public int Amount { get; set; }
}

public class CategoryRequest
{
    public long? Id { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; }
    public string Color { get; set; }
    public int Amount { get; set; }
}

public class UpdateBudgetRequest
{
    public int Income { get; set; }
    public List<CategoryRequest> Categories { get; set; }
    public List<int> DeletedAllocationIds { get; set; }
}