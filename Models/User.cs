namespace FinanceTracker.API.Models
{
    public class User
    {
        public int Id { get; set; }                  // Primary key
        public string Email { get; set; } = "";      // User's email address
        public string PasswordHash { get; set; } = ""; // Secure password hash
        public string PasswordSalt { get; set; } = ""; // Secure password salt
        public string FirstName { get; set; } = "";  // User's first name
        public string LastName { get; set; } = "";   // User's last name
        public string PhoneNumber { get; set; } = ""; // User's phone number
        public bool HasBudget { get; set; } = false;// User has a budget
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}