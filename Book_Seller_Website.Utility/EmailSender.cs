using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace Book_Seller_Website.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // fake task vi trong middleware khong su dung addidentitydefault nua ma dung addidentity
           
            var emailSend = new MimeMessage();
            emailSend.From.Add(MailboxAddress.Parse("hello"));
            emailSend.To.Add(MailboxAddress.Parse(email));
            emailSend.Subject = subject;
            emailSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            // send email
            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("email@gmail.com", "password");
                smtp.Send(emailSend);
                smtp.Disconnect(true);
            }
             return Task.CompletedTask;
            // google da tat chuc nang cho phep dang nhap tu ung dung khong an toan
        }
    }
}
