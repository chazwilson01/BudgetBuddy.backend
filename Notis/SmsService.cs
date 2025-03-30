using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;


//3Q18JEVAAZ91V6GTYZGHEGJP
namespace FinanceTracker.API.Notis
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Log the initialization
            _logger.LogInformation("Initializing Twilio client");

            try
            {
                TwilioClient.Init(
                    _configuration["Sms:AccountSid"],
                    _configuration["Sms:AuthToken"]
                );
                _logger.LogInformation("Twilio client initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Twilio client");
                throw;
            }
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // Ensure phone number is in E.164 format
                if (!phoneNumber.StartsWith("+"))
                {
                    _logger.LogWarning("Phone number {Phone} is not in E.164 format, attempting to fix", phoneNumber);
                    phoneNumber = "+" + phoneNumber.TrimStart('0');
                }

                _logger.LogInformation("Sending SMS to {Phone} with message: {Message}", phoneNumber, message);

                var smsMessage = await MessageResource.CreateAsync(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(_configuration["Sms:PhoneNumber"]),
                    to: new Twilio.Types.PhoneNumber(phoneNumber)
                );

                _logger.LogInformation("SMS sent successfully. Message SID: {SID}, Status: {Status}",
                    smsMessage.Sid, smsMessage.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {Phone}", phoneNumber);
                throw;
            }
        }
    }
}