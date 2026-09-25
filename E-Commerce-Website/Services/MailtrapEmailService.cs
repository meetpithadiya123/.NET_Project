using System.Net;
using System.Net.Mail;
using System.Text;
using E_Commerce_Website.Models;

namespace E_Commerce_Website.Services
{
    public class MailtrapEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailtrapEmailService> _logger;

        public MailtrapEmailService(IConfiguration configuration, ILogger<MailtrapEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            // Read standard setup parameters from configuration
            string host = _configuration["Mailtrap:Host"] ?? "live.smtp.mailtrap.io";
            string portValue = _configuration["Mailtrap:Port"] ?? "587";
            int port = int.TryParse(portValue, out int p) ? p : 587;
            string username = _configuration["Mailtrap:Username"] ?? "api";
            string senderEmail = _configuration["Mailtrap:SenderEmail"] ?? "hello@demomailtrap.co";
            string senderName = _configuration["Mailtrap:SenderName"] ?? "UrbanCart Store";

            // Read sensitive API Token from Environment Variable, with fallback to configuration
            string? apiToken = Environment.GetEnvironmentVariable("MAILTRAP_API_TOKEN");
            if (string.IsNullOrWhiteSpace(apiToken))
            {
                apiToken = _configuration["MAILTRAP_API_TOKEN"] ?? _configuration["Mailtrap:ApiToken"];
            }

            if (string.IsNullOrWhiteSpace(apiToken))
            {
                _logger.LogError("Mailtrap API token is not configured. Please set the MAILTRAP_API_TOKEN environment variable.");
                throw new InvalidOperationException("Mailtrap API token is missing. Please set the MAILTRAP_API_TOKEN environment variable.");
            }

            try
            {
                _logger.LogInformation("Connecting to Mailtrap SMTP at {Host}:{Port} with user '{Username}'...", host, port, username);

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, apiToken),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Order receipt email successfully dispatched to {Recipient}. View delivery logs at https://mailtrap.io/sending/email_logs", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via Mailtrap SMTP to {Recipient}. Inspect logs at https://mailtrap.io/sending/email_logs", toEmail);
                throw;
            }
        }

        public async Task SendOrderConfirmationEmailAsync(Order order, string recipientEmail)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                _logger.LogWarning("Cannot send order confirmation email: recipient email is empty.");
                return;
            }

            string subject = $"Order #{order.OrderId} Payment Successful - Transaction Receipt [{order.TransactionId}]";
            string htmlBody = GenerateOrderReceiptHtml(order);

            await SendEmailAsync(recipientEmail, subject, htmlBody, isHtml: true);
        }

        private string GenerateOrderReceiptHtml(Order order)
        {
            var sb = new StringBuilder();
            string customerName = !string.IsNullOrWhiteSpace(order.Customer?.customer_name) ? order.Customer.customer_name : "Valued Customer";
            string formattedDate = order.OrderDate.ToString("f");

            sb.Append(@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>Order Confirmation & Payment Receipt</title>
<style>
    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f1f5f9; margin: 0; padding: 24px; color: #1e293b; }
    .email-container { max-width: 640px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08); border: 1px solid #e2e8f0; }
    .header-bar { background: linear-gradient(135deg, #4338ca 0%, #312e81 100%); padding: 32px 28px; text-align: center; color: #ffffff; }
    .header-bar h1 { margin: 0 0 8px 0; font-size: 24px; font-weight: 800; letter-spacing: -0.5px; }
    .header-bar p { margin: 0; color: #c7d2fe; font-size: 14px; }
    .badge-success { display: inline-block; background: #10b981; color: #ffffff; padding: 6px 14px; border-radius: 9999px; font-size: 12px; font-weight: 700; text-transform: uppercase; margin-top: 12px; }
    .content-body { padding: 28px; }
    .greeting { font-size: 16px; margin-bottom: 20px; line-height: 1.5; }
    .txn-card { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 18px; margin-bottom: 24px; }
    .txn-row { display: flex; justify-content: space-between; padding: 6px 0; font-size: 13px; border-bottom: 1px dashed #cbd5e1; }
    .txn-row:last-child { border-bottom: none; }
    .txn-label { color: #64748b; font-weight: 600; }
    .txn-val { font-weight: 700; color: #0f172a; text-align: right; }
    .table-container { width: 100%; border-collapse: collapse; margin-bottom: 24px; }
    .table-container th { text-align: left; padding: 10px 12px; background: #f1f5f9; color: #475569; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px; }
    .table-container td { padding: 12px; border-bottom: 1px solid #f1f5f9; font-size: 14px; }
    .total-box { background: #eef2ff; border: 1px solid #c7d2fe; border-radius: 8px; padding: 16px; margin-bottom: 24px; text-align: right; }
    .total-box .grand-total { font-size: 22px; font-weight: 800; color: #3730a3; }
    .shipping-box { background: #ffffff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 16px; margin-bottom: 24px; }
    .shipping-box h4 { margin: 0 0 8px 0; font-size: 14px; color: #334155; text-transform: uppercase; }
    .footer-bar { background: #f8fafc; border-top: 1px solid #e2e8f0; padding: 20px 28px; text-align: center; font-size: 12px; color: #64748b; }
    .footer-bar a { color: #4338ca; text-decoration: none; }
</style>
</head>
<body>
<div class='email-container'>
    <div class='header-bar'>
        <h1>UrbanCart Store</h1>
        <p>Official Transaction &amp; Payment Confirmation</p>
        <span class='badge-success'>Payment Verified &bull; ");
            sb.Append(WebUtility.HtmlEncode(order.PaymentStatus));
            sb.Append(@"</span>
    </div>

    <div class='content-body'>
        <p class='greeting'>Hello <strong>");
            sb.Append(WebUtility.HtmlEncode(customerName));
            sb.Append(@"</strong>,<br/>Thank you for your purchase! Your payment has been received and processed successfully. Below are your full transaction details.</p>

        <div class='txn-card'>
            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 6px 0; color: #64748b; font-size: 13px; font-weight: 600;'>Transaction Reference</td>
                    <td style='padding: 6px 0; color: #4338ca; font-size: 13px; font-weight: 800; text-align: right; font-family: monospace;'>");
            sb.Append(WebUtility.HtmlEncode(order.TransactionId));
            sb.Append(@"</td>
                </tr>
                <tr>
                    <td style='padding: 6px 0; color: #64748b; font-size: 13px; font-weight: 600;'>Order ID</td>
                    <td style='padding: 6px 0; color: #0f172a; font-size: 13px; font-weight: 700; text-align: right;'>#");
            sb.Append(order.OrderId);
            sb.Append(@"</td>
                </tr>
                <tr>
                    <td style='padding: 6px 0; color: #64748b; font-size: 13px; font-weight: 600;'>Payment Method</td>
                    <td style='padding: 6px 0; color: #0f172a; font-size: 13px; font-weight: 700; text-align: right;'>");
            sb.Append(WebUtility.HtmlEncode(order.PaymentMode));
            sb.Append(@"</td>
                </tr>
                <tr>
                    <td style='padding: 6px 0; color: #64748b; font-size: 13px; font-weight: 600;'>Transaction Date</td>
                    <td style='padding: 6px 0; color: #0f172a; font-size: 13px; font-weight: 700; text-align: right;'>");
            sb.Append(formattedDate);
            sb.Append(@"</td>
                </tr>
                <tr>
                    <td style='padding: 6px 0; color: #64748b; font-size: 13px; font-weight: 600;'>Payment Status</td>
                    <td style='padding: 6px 0; color: #10b981; font-size: 13px; font-weight: 800; text-align: right;'>");
            sb.Append(WebUtility.HtmlEncode(order.PaymentStatus));
            sb.Append(@"</td>
                </tr>
            </table>
        </div>

        <h3 style='font-size: 16px; margin: 0 0 12px 0; color: #0f172a;'>Purchased Items</h3>
        <table class='table-container'>
            <thead>
                <tr>
                    <th>Item</th>
                    <th style='text-align: center;'>Qty</th>
                    <th style='text-align: right;'>Unit Price</th>
                    <th style='text-align: right;'>Subtotal</th>
                </tr>
            </thead>
            <tbody>");

            if (order.OrderItems != null && order.OrderItems.Any())
            {
                foreach (var item in order.OrderItems)
                {
                    sb.Append(@"
                <tr>
                    <td><strong>");
                    sb.Append(WebUtility.HtmlEncode(item.ProductName));
                    sb.Append(@"</strong></td>
                    <td style='text-align: center;'>");
                    sb.Append(item.Quantity);
                    sb.Append(@"</td>
                    <td style='text-align: right;'>₹");
                    sb.Append(item.UnitPrice.ToString("N2"));
                    sb.Append(@"</td>
                    <td style='text-align: right; font-weight: 700;'>₹");
                    sb.Append(item.SubTotal.ToString("N2"));
                    sb.Append(@"</td>
                </tr>");
                }
            }
            else
            {
                sb.Append(@"
                <tr>
                    <td colspan='4' style='text-align: center; color: #64748b;'>Order item details recorded.</td>
                </tr>");
            }

            sb.Append(@"
            </tbody>
        </table>

        <div class='total-box'>
            <div style='font-size: 13px; color: #475569; margin-bottom: 4px;'>Grand Total Paid</div>
            <div class='grand-total'>₹");
            sb.Append(order.TotalAmount.ToString("N2"));
            sb.Append(@"</div>
        </div>

        <div class='shipping-box'>
            <h4>Delivery Address</h4>
            <p style='margin: 0; font-size: 14px; line-height: 1.5; color: #334155;'>
                <strong>");
            sb.Append(WebUtility.HtmlEncode(customerName));
            sb.Append(@"</strong><br/>");
            sb.Append(WebUtility.HtmlEncode(order.ShippingAddress));
            sb.Append(@", ");
            sb.Append(WebUtility.HtmlEncode(order.City));
            if (!string.IsNullOrWhiteSpace(order.PostalCode))
            {
                sb.Append(@" - ");
                sb.Append(WebUtility.HtmlEncode(order.PostalCode));
            }
            sb.Append(@"<br/>Phone: ");
            sb.Append(WebUtility.HtmlEncode(order.Phone));
            sb.Append(@"
            </p>
        </div>
    </div>

    <div class='footer-bar'>
        <p style='margin: 0 0 6px 0;'>This is an automated transaction receipt from UrbanCart Store.</p>
        <p style='margin: 0 0 6px 0;'>Mailtrap SMTP delivery logs can be tracked at <a href='https://mailtrap.io/sending/email_logs' target='_blank'>https://mailtrap.io/sending/email_logs</a>.</p>
        <p style='margin: 0;'>Thank you for shopping with us!</p>
    </div>
</div>
</body>
</html>");

            return sb.ToString();
        }
    }
}
