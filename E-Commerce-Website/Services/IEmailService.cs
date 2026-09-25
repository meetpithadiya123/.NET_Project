using E_Commerce_Website.Models;

namespace E_Commerce_Website.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task SendOrderConfirmationEmailAsync(Order order, string recipientEmail);
    }
}
