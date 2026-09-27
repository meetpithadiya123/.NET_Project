using System.Net;
using System.Net.Mail;
using System.Text;
using E_Commerce_Website.Models;

namespace E_Commerce_Website.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            string host = _configuration["EmailSettings:Host"] ?? "smtp.gmail.com";
            string portValue = _configuration["EmailSettings:Port"] ?? "587";
            int port = int.TryParse(portValue, out int p) ? p : 587;
            string username = _configuration["EmailSettings:Username"] ?? "urbancart797@gmail.com";
            string password = _configuration["EmailSettings:Password"] ?? string.Empty;
            string senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "urbancart797@gmail.com";
            string senderName = _configuration["EmailSettings:SenderName"] ?? "UrbanCart Store";

            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogError("Email password or app password is not configured in EmailSettings:Password.");
                throw new InvalidOperationException("Email password is missing.");
            }

            string cleanPassword = password.Replace(" ", "").Trim();

            _logger.LogInformation("Dispatching email directly via {Host}:{Port} from {Sender} to {Recipient}...", 
                host, port, senderEmail, toEmail);

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, cleanPassword),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 15000
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
            _logger.LogInformation("Confirmation email successfully delivered directly to {Recipient} via {Sender}.", toEmail, senderEmail);
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
            string formattedDate = order.OrderDate.ToString("dd MMM yyyy, hh:mm tt");
            string customerEmail = !string.IsNullOrWhiteSpace(order.ShippingEmail) ? order.ShippingEmail : (order.Customer?.customer_email ?? "N/A");
            int totalItemsCount = order.OrderItems?.Sum(i => i.Quantity) ?? 0;

            sb.Append(@"<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>Order Confirmation &amp; Payment Receipt</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; color: #1e293b; -webkit-font-smoothing: antialiased;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #f1f5f9; padding: 30px 10px;'>
        <tr>
            <td align='center'>
                <!-- Main Card Container -->
                <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='max-width: 620px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08); border: 1px solid #e2e8f0;'>
                    
                    <!-- Header Banner -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #1e1b4b 0%, #312e81 60%, #4338ca 100%); padding: 36px 32px; text-align: center;'>
                            <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0'>
                                <tr>
                                    <td align='center'>
                                        <!-- Brand Logo & Name -->
                                        <div style='font-size: 26px; font-weight: 900; color: #ffffff; letter-spacing: -0.5px; margin-bottom: 6px;'>
                                            Urban<span style='color: #a5b4fc;'>Cart</span>
                                        </div>
                                        <div style='font-size: 13px; color: #c7d2fe; text-transform: uppercase; letter-spacing: 1.5px; margin-bottom: 18px;'>
                                            Official Payment &amp; Order Receipt
                                        </div>
                                        <!-- Verified Status Pill -->
                                        <table role='presentation' cellspacing='0' cellpadding='0' border='0' style='margin: 0 auto;'>
                                            <tr>
                                                <td style='background-color: #10b981; color: #ffffff; padding: 6px 18px; border-radius: 9999px; font-size: 13px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px;'>
                                                    &#10003; Payment Successful &bull; Order #");
            sb.Append(order.OrderId);
            sb.Append(@"
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 32px 28px;'>
                            <!-- Greeting Box -->
                            <div style='font-size: 16px; line-height: 1.6; color: #334155; margin-bottom: 24px;'>
                                Hello <strong style='color: #0f172a;'>");
            sb.Append(WebUtility.HtmlEncode(customerName));
            sb.Append(@"</strong>,<br/>
                                Thank you for shopping with us! We have received your payment of <strong style='color: #4f46e5;'>₹");
            sb.Append(order.TotalAmount.ToString("N2"));
            sb.Append(@"</strong>. Your order is confirmed and our team is preparing it for shipment.
                            </div>

                            <!-- Transaction Overview Quick Grid -->
                            <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; margin-bottom: 26px; padding: 14px;'>
                                <tr>
                                    <td style='padding: 8px 12px; border-bottom: 1px solid #edf2f7; font-size: 13px; color: #64748b; font-weight: 600;'>Order ID</td>
                                    <td align='right' style='padding: 8px 12px; border-bottom: 1px solid #edf2f7; font-size: 13px; color: #0f172a; font-weight: 700;'>#");
            sb.Append(order.OrderId);
            sb.Append(@"</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 12px; border-bottom: 1px solid #edf2f7; font-size: 13px; color: #64748b; font-weight: 600;'>Transaction ID</td>
                                    <td align='right' style='padding: 8px 12px; border-bottom: 1px solid #edf2f7; font-size: 12px; font-family: monospace; color: #4338ca; font-weight: 700;'>");
            sb.Append(WebUtility.HtmlEncode(order.TransactionId));
            sb.Append(@"</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 12px; border-bottom: 1px solid #edf2f7; font-size: 13px; color: #64748b; font-weight: 600;'>Date &amp; Time</td>
                                    <td align='right' style='padding: 8px 12px; border-bottom: 1px solid #edf2f7; font-size: 13px; color: #0f172a; font-weight: 600;'>");
            sb.Append(formattedDate);
            sb.Append(@"</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 12px; border-bottom: 1px solid #edf2f7; font-size: 13px; color: #64748b; font-weight: 600;'>Payment Method</td>
                                    <td align='right' style='padding: 8px 12px; border-bottom: 1px solid #edf2f7; font-size: 13px; color: #0f172a; font-weight: 700;'>");
            sb.Append(WebUtility.HtmlEncode(order.PaymentMode));
            sb.Append(@"</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 12px; font-size: 13px; color: #64748b; font-weight: 600;'>Recipient Email</td>
                                    <td align='right' style='padding: 8px 12px; font-size: 13px; color: #4f46e5; font-weight: 600;'>");
            sb.Append(WebUtility.HtmlEncode(customerEmail));
            sb.Append(@"</td>
                                </tr>
                            </table>

                            <!-- Items Section Heading -->
                            <div style='font-size: 15px; font-weight: 800; color: #0f172a; margin-bottom: 12px; text-transform: uppercase; letter-spacing: 0.5px;'>
                                Purchased Items (");
            sb.Append(totalItemsCount);
            sb.Append(@")
                            </div>

                            <!-- Items Table -->
                            <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse; margin-bottom: 24px; border: 1px solid #e2e8f0; border-radius: 10px; overflow: hidden;'>
                                <thead>
                                    <tr style='background-color: #f1f5f9;'>
                                        <th align='left' style='padding: 10px 14px; font-size: 12px; color: #475569; text-transform: uppercase; letter-spacing: 0.5px; border-bottom: 1px solid #e2e8f0;'>Product</th>
                                        <th align='center' style='padding: 10px 10px; font-size: 12px; color: #475569; text-transform: uppercase; letter-spacing: 0.5px; border-bottom: 1px solid #e2e8f0;'>Qty</th>
                                        <th align='right' style='padding: 10px 10px; font-size: 12px; color: #475569; text-transform: uppercase; letter-spacing: 0.5px; border-bottom: 1px solid #e2e8f0;'>Price</th>
                                        <th align='right' style='padding: 10px 14px; font-size: 12px; color: #475569; text-transform: uppercase; letter-spacing: 0.5px; border-bottom: 1px solid #e2e8f0;'>Total</th>
                                    </tr>
                                </thead>
                                <tbody>");

            if (order.OrderItems != null && order.OrderItems.Any())
            {
                foreach (var item in order.OrderItems)
                {
                    sb.Append(@"
                                    <tr>
                                        <td style='padding: 12px 14px; border-bottom: 1px solid #f1f5f9; font-size: 14px; color: #0f172a; font-weight: 600;'>");
                    sb.Append(WebUtility.HtmlEncode(item.ProductName));
                    sb.Append(@"</td>
                                        <td align='center' style='padding: 12px 10px; border-bottom: 1px solid #f1f5f9; font-size: 13px; color: #475569;'>");
                    sb.Append(item.Quantity);
                    sb.Append(@"</td>
                                        <td align='right' style='padding: 12px 10px; border-bottom: 1px solid #f1f5f9; font-size: 13px; color: #64748b;'>₹");
                    sb.Append(item.UnitPrice.ToString("N2"));
                    sb.Append(@"</td>
                                        <td align='right' style='padding: 12px 14px; border-bottom: 1px solid #f1f5f9; font-size: 14px; font-weight: 700; color: #0f172a;'>₹");
                    sb.Append(item.SubTotal.ToString("N2"));
                    sb.Append(@"</td>
                                    </tr>");
                }
            }
            else
            {
                sb.Append(@"
                                    <tr>
                                        <td colspan='4' align='center' style='padding: 16px; color: #64748b; font-size: 13px;'>Order item details recorded.</td>
                                    </tr>");
            }

            sb.Append(@"
                                </tbody>
                            </table>

                            <!-- Payment & Total Summary Card -->
                            <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #eef2ff; border: 1.5px solid #c7d2fe; border-radius: 12px; margin-bottom: 26px; padding: 18px 20px;'>
                                <tr>
                                    <td style='font-size: 13px; color: #4338ca; font-weight: 600; padding-bottom: 6px;'>Subtotal</td>
                                    <td align='right' style='font-size: 14px; color: #3730a3; font-weight: 700; padding-bottom: 6px;'>₹");
            sb.Append(order.TotalAmount.ToString("N2"));
            sb.Append(@"</td>
                                </tr>
                                <tr>
                                    <td style='font-size: 13px; color: #4338ca; font-weight: 600; padding-bottom: 10px;'>Standard Delivery</td>
                                    <td align='right' style='font-size: 13px; color: #10b981; font-weight: 700; padding-bottom: 10px;'>FREE</td>
                                </tr>
                                <tr>
                                    <td style='border-top: 1.5px dashed #a5b4fc; padding-top: 10px; font-size: 16px; color: #1e1b4b; font-weight: 800;'>Grand Total Paid</td>
                                    <td align='right' style='border-top: 1.5px dashed #a5b4fc; padding-top: 10px; font-size: 22px; color: #4338ca; font-weight: 900;'>₹");
            sb.Append(order.TotalAmount.ToString("N2"));
            sb.Append(@"</td>
                                </tr>
                            </table>

                            <!-- Shipping Destination Card -->
                            <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px; margin-bottom: 26px; padding: 18px 20px;'>
                                <tr>
                                    <td>
                                        <div style='font-size: 12px; color: #64748b; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 8px;'>
                                            &#128230; Delivery Destination
                                        </div>
                                        <div style='font-size: 14px; font-weight: 700; color: #0f172a; margin-bottom: 4px;'>");
            sb.Append(WebUtility.HtmlEncode(customerName));
            sb.Append(@"</div>
                                        <div style='font-size: 13px; color: #475569; line-height: 1.5;'>");
            sb.Append(WebUtility.HtmlEncode(order.ShippingAddress));
            sb.Append(@", ");
            sb.Append(WebUtility.HtmlEncode(order.City));
            if (!string.IsNullOrWhiteSpace(order.PostalCode))
            {
                sb.Append(@" - ");
                sb.Append(WebUtility.HtmlEncode(order.PostalCode));
            }
            sb.Append(@"<br/><strong>Contact Phone:</strong> ");
            sb.Append(WebUtility.HtmlEncode(order.Phone));
            sb.Append(@"
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <!-- Estimated Delivery Notice -->
                            <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 10px; margin-bottom: 26px; padding: 14px 18px;'>
                                <tr>
                                    <td>
                                        <div style='font-size: 13px; font-weight: 700; color: #166534;'>
                                            &#128666; Estimated Delivery: 3 - 5 Business Days
                                        </div>
                                        <div style='font-size: 12px; color: #15803d; margin-top: 2px;'>
                                            You can track your order status anytime by logging into your account.
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 24px 28px; text-align: center;'>
                            <div style='font-size: 12px; color: #64748b; line-height: 1.6; margin-bottom: 8px;'>
                                This is an automated transaction receipt for your records.<br/>
                                If you have questions regarding this purchase, please contact us at 
                                <a href='mailto:urbancart797@gmail.com' style='color: #4f46e5; text-decoration: none; font-weight: 600;'>urbancart797@gmail.com</a>.
                            </div>
                            <div style='font-size: 11px; color: #94a3b8;'>
                                &copy; ");
            sb.Append(DateTime.UtcNow.Year);
            sb.Append(@" UrbanCart Store. All rights reserved. &bull; 256-Bit SSL Encrypted
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>");

            return sb.ToString();
        }
    }
}
