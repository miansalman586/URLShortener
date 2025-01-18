using System.Net.Mail;
using System.Net;
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace URLShortener.Helpers
{
    public class TaskService
    {
        public readonly IServiceProvider _serviceProvider;

        public TaskService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(Action action)
        {
            try
            {
                await Task.Run(action);
            }
            catch(Exception ex)
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("miansalman586@gmail.com");
                mail.To.Add("miansalman586@gmail.com");
                mail.Subject = "URL Shortener - Exception";
                mail.Body = ex.ToString();
                mail.IsBodyHtml = false;

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("miansalman586@gmail.com", "xdhl byef buhp gwxn"),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(mail);
            }
        }
    }
}
