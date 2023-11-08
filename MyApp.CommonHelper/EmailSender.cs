using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.CommonHelper
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var toEmail = new MimeMessage();
            toEmail.From.Add(MailboxAddress.Parse("MyGmailId"));
            toEmail.To.Add(MailboxAddress.Parse(email));
            toEmail.Subject=subject;
            toEmail.Body = new TextPart(MimeKit.Text.TextFormat.Html) {Text= htmlMessage};

            using(var emailclient= new SmtpClient())
            {
                emailclient.Connect("smtp.gmail.com",587,MailKit.Security.SecureSocketOptions.StartTls);
                emailclient.Authenticate("", "");
                emailclient.SendAsync(toEmail);
                emailclient.Disconnect(true);
            }
            return Task.CompletedTask;
        }
    }
}
