using System.Net;
using System.Net.Mail;
using GammarApplication.Interfaces;

namespace GammarInfrastructure.Services;

public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var fromAddress = new MailAddress("oko93780@gmail.com", "Gammar Tiếng Nhật");
        var toAddress = new MailAddress(toEmail);
        const string fromPassword = "tbju ivmo xjef hbjc"; // App password provided by user

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        await smtp.SendMailAsync(message);
    }
}
