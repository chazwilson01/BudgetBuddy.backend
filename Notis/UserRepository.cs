
using FinanceTracker.API.Data;
using FinanceTracker.API.Models;
using FinanceTracker.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // This enables ToListAsync()

namespace FinanceTracker.API.Notis
{

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<User>> GetUsersForBiweeklyNotifications()
        {
            // Query your database for users who should receive notifications
            return await _dbContext.Users
               .Where(u => u.EmailNotificationsEnabled == true || u.SmsNotificationsEnabled == true)
               .ToListAsync();
        }
    }

}