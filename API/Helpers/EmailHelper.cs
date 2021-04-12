using API.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace API.Helpers
{
    public static class EmailHelper
    {
        public static async Task SendConfirmationMail(string userEmail, string confirmationLink)
        {
            await SendMailAsync(userEmail, "Life Changer - confirm your email",
                $"Hello. Click the link to activate your account: <a href=\"{confirmationLink}\">Link</a>");
        }
        public static async Task SendNewEmailConfirmationMail(string userEmail, string confirmationLink)
        {
            await SendMailAsync(userEmail, "Life Changer - confirm your new email",
                $"Hello. You just changed the email address on your account. Please click the link to confirm your new email: <a href=\"{confirmationLink}\">Link</a>");
        }
        public static async Task SendPasswordChangedMail(string userEmail)
        {
            await SendMailAsync(userEmail, "Life Changer - your password has been changed",
                "Hello. Your password has been changed. If you didn't do it, please contact with our support immediately.");
        }
        public static async Task SendMailAsync(string toAddress, string subject, string body, bool isBodyHtml = true)
        {

            MailMessage mailMessage = new();
            mailMessage.From = new MailAddress("lifechangertests@gmail.com");
            mailMessage.To.Add(new MailAddress(toAddress));

            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = isBodyHtml;
            mailMessage.Body = body;

            SmtpClient client = new("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential("lifechangertests@gmail.com", "LifeChanger");

            await client.SendMailAsync(mailMessage);
        }

    }
}
