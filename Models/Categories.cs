namespace FinanceTracker.API.Models
{
    public class Categories
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int Amount { get; set; } = 0;
        public string Color { get; set; } = "";
        public string Category { get; set; } = "";

    }
}