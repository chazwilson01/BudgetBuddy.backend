using FinanceTracker.API.Models;


namespace FinanceTracker.API.Notis
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetUsersForBiweeklyNotifications();
        // You might add other methods as needed
    }
}