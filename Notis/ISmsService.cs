using System.Threading.Tasks;

namespace FinanceTracker.API.Notis
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
    }
}