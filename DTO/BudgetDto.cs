namespace FinanceTracker.API.DTO
{
    public class BudgetDto
    {
        public int Income { get; set; }
        public List<CreateCategoryRequest> Categories { get; set; }
    }
}