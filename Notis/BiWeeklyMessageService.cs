using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.API.Notis
{
    public class BiweeklyMessageService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<BiweeklyMessageService> _logger;

        public BiweeklyMessageService(IServiceProvider services, ILogger<BiweeklyMessageService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Biweekly Message Service running.");

            using PeriodicTimer timer = new(TimeSpan.FromHours(1));

            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (IsBiweeklyTriggerTime())
                    {
                        await SendMessagesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending scheduled messages.");
                }
            }
        }

        private bool IsBiweeklyTriggerTime()
        {
            return true;
            //var now = DateTime.UtcNow;
            //return now.DayOfWeek == DayOfWeek.Monday &&
            //      now.Hour == 9 &&
            //      now.Minute < 15 &&
            //      (now.Day / 14) != ((now.Day - 7) / 14);
        }

        private async Task SendMessagesAsync()
        {
            using var scope = _services.CreateScope();

            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var users = await userRepository.GetUsersForBiweeklyNotifications();

            foreach (var user in users)
            {
                if (user.EmailNotificationsEnabled)
                    await emailService.SendEmailAsync(user.Email, "Biweekly Update", "Your biweekly update is here!");

                if (user.SmsNotificationsEnabled)
                    await smsService.SendSmsAsync(user.PhoneNumber, "Your biweekly update is here!");
            }

            _logger.LogInformation("Biweekly messages sent successfully at {time}", DateTimeOffset.Now);
        }
    }
}