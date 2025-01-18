using Azure.Core;
using System.Net.Mail;
using System.Net;

namespace URLShortener.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;

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
