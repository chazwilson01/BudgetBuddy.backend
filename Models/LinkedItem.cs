using System;

namespace FinanceTracker.API.Models
{
    public class LinkedItem
    {
        public int Id { get; set; }                  // Primary key
        public int UserId { get; set; }    // App user this token belongs to
        public string AccessToken { get; set; } = ""; // Secure Plaid token
        public string ItemId { get; set; } = "";     // Plaid item ID
        public string InstitutionName { get; set; } = ""; // e.g., "Chase"
        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
        public bool IsReady { get; set; } = false;

    }
}
