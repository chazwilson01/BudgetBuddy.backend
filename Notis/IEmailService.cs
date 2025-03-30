using System.Threading.Tasks;

namespace FinanceTracker.API.Notis
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}