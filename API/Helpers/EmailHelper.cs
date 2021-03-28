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
        public static async Task<bool> SendConfirmationMail(string userEmail, string confirmationLink)
        {
            MailMessage mailMessage = new();
            mailMessage.From = new MailAddress("lifechangertests@gmail.com");
            mailMessage.To.Add(new MailAddress(userEmail));

            mailMessage.Subject = "Life Changer - confirm your email";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = $"Hello. Click the link to activate your account: <a href=\"{confirmationLink}\">Link</a>";

            SmtpClient client = new("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential("lifechangertests@gmail.com", "LifeChanger");

            try
            {
                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending message" + ex.Message);
            }
            return false;
        }

    }
}
