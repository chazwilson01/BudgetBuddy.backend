namespace FinanceTracker.API.Models
{ 
	public class Budget
	{
		public int Id { get; set; }
		public int userId { get; set; }
		public int Income { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}