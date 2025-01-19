using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using URLShortener.Models;
using System.Text;
using URLShortener.Data;
using URLShortener.Entity;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using Azure.Core;
using URLShortener.Helpers;

namespace URLShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly ContactDbContext _contactDbContext;
        private readonly LangTransHelper _langTransHelper;

        public HomeController(AppDbContext appDbContext, ContactDbContext contactDbContext, LangTransHelper langTransHelper)
        {
            _appDbContext = appDbContext;
            _contactDbContext = contactDbContext;
            _langTransHelper = langTransHelper;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["IsLang"] = true;
            return await Task.Run(View);
        }

        [NonAction]
        private long ConvertBase36ToDecimal(string base36String)
        {
            string base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            base36String = base36String.ToUpper();
            long result = 0;

            foreach (char c in base36String)
            {
                int value = base36Chars.IndexOf(c);
                result = result * 36 + value;
            }

            return result;
        }

        [NonAction]
        private string ConvertToBase36(long decimalNumber)
        {
            string base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (decimalNumber == 0)
                return "0";

            string result = string.Empty;
            while (decimalNumber > 0)
            {
                int remainder = (int)(decimalNumber % 36);
                result = base36Chars[remainder] + result;
                decimalNumber /= 36;
            }

            return result;
        }

        [NonAction]
        private bool IsIndexingCrawler()
        {
            string userAgent = HttpContext.Request.Headers["User-Agent"].ToString().ToLower();

            return userAgent.Contains("googlebot") ||
                userAgent.Contains("ahrefsbot") ||
                userAgent.Contains("bingbot") ||
                userAgent.Contains("yandexbot");
        }

        [Route("{url}")]
        public async Task<IActionResult> Index(string url)
        {
            if (((IList<Google.Cloud.Translation.V2.Language>?)HttpContext.Items["langs"]).Any(e => e.Code == url))
            {
                ViewData["IsLang"] = true;

                return View();
            }

            if (IsIndexingCrawler())
            {
                return RedirectPermanent("/");
            }

            long id = ConvertBase36ToDecimal(url);
            var shortedURL = await _appDbContext.ShortedURL.Where(e => e.ID == id).SingleOrDefaultAsync();

            if (shortedURL == null)
            {
                return NotFound();
            }

            if (shortedURL.IsDeleted == true)
            {
                return NoContent();
            }

            shortedURL.View = shortedURL.View + 1;
            shortedURL.LastView = DateTime.Now;
            _appDbContext.ShortedURL.Update(shortedURL);

            await _appDbContext.SaveChangesAsync();

            return RedirectPermanent(shortedURL.LongURL);
        }

        [NonAction]
        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        [HttpPost]
        public async Task<IActionResult> ShortURL(ShortURLModel request)
        {
            if (!IsValidUrl(request.URL))
            {
                return BadRequest(new { message = _langTransHelper.TransText("InvalidURL.") });
            }

            bool isSHort = false;
            var url = await _appDbContext.ShortedURL.Where(e => e.LongURL == request.URL).SingleOrDefaultAsync();
            if (url == null)
            {
                url = new ShortedURL();
                url.LongURL = request.URL;
                url.CreatedDate = DateTime.Now;
                url.LastView = DateTime.Now;
                url.View = 0;

                await _appDbContext.ShortedURL.AddAsync(url);
                await _appDbContext.SaveChangesAsync();

                isSHort = true;
            }

            string urlId = ConvertToBase36(url.ID);
            string shortedURL = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/{urlId}";

            if (string.IsNullOrEmpty(url.CustomizedURL))
            {
                return Ok(new { isSHort, shortedURL, url.View, LastView = url.LastView.ToString(), CreatedDate = url.CreatedDate.ToString() });
            }

            return Ok(new { isSHort, customizedURL = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/c/{url.CustomizedURL}", shortedURL, url.View, LastView = url.LastView.ToString(), CreatedDate = url.CreatedDate.ToString() });
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        public IActionResult TermsOfService()
        {
            return View();
        }

        public IActionResult Disclaimer()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult DonateUs()
        {
            return View();
        }

        public IActionResult CustomizeURL()
        {
            return View();
        }

        [HttpGet("c/{url}")]
        public async Task<IActionResult> CustomizeURL(string url)
        {
            if (IsIndexingCrawler())
            {
                return RedirectPermanent("/");
            }

            var shortedURL = await _appDbContext.ShortedURL.Where(e => e.CustomizedURL == url).SingleOrDefaultAsync();
            if (shortedURL == null)
            {
                return NotFound();
            }

            if (shortedURL.IsDeleted == true)
            {
                return NoContent();
            }

            shortedURL.View = shortedURL.View + 1;
            shortedURL.LastView = DateTime.Now;
            _appDbContext.ShortedURL.Update(shortedURL);

            await _appDbContext.SaveChangesAsync();

            return RedirectPermanent(shortedURL.LongURL);
        }

        [HttpPost]
        public async Task<IActionResult> CustomizeURL(CustomizeURLModel request)
        {
            if (!IsValidUrl(request.OrignalURL))
            {
                return BadRequest(new { message = _langTransHelper.TransText("InvalidURL.") });
            }

            var url = await _appDbContext.ShortedURL.Where(e => e.LongURL == request.OrignalURL).SingleOrDefaultAsync();
            bool isSHort = false;
            if (url == null)
            {
                url = new ShortedURL();
                url.LongURL = request.OrignalURL;
                url.CreatedDate = DateTime.Now;
                url.LastView = DateTime.Now;
                url.View = 0;
                url.CustomizedURL = request.URL.Replace($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/c/", string.Empty);

                var isExists = await _appDbContext.ShortedURL.Where(e => e.CustomizedURL == url.CustomizedURL).SingleOrDefaultAsync();
                if (isExists != null)
                {
                    return BadRequest(new { message = _langTransHelper.TransText("URLTaken") });
                }

                await _appDbContext.ShortedURL.AddAsync(url);
                await _appDbContext.SaveChangesAsync();

                isSHort = true;
            }
            else
            {
                var custURL = request.URL.Replace($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/c/", string.Empty);

                var isExistsUpdate = await _appDbContext.ShortedURL.Where(e => e.CustomizedURL == custURL).SingleOrDefaultAsync();
                if (isExistsUpdate == null)
                {
                    url.CustomizedURL = custURL;

                    _appDbContext.ShortedURL.Update(url);
                    await _appDbContext.SaveChangesAsync();
                }
            }

            string urlId = ConvertToBase36(url.ID);
            string shortedURL = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/{urlId}";

            return Ok(new { isSHort, CustomizedURL = request.URL, shortedURL, url.View, LastView = url.LastView.ToString(), CreatedDate = url.CreatedDate.ToString() });
        }

        [HttpPost]
        public async Task<IActionResult> ContactUs(ContactUsModel request)
        {
            StringBuilder sr = new StringBuilder();
            sr.Append($"<b>Name:</b> {request.FirstName} {request.LastName}");
            sr.Append("<br />");
            sr.Append($"<b>Email:</b> {request.EmailAddress}");
            sr.Append("<br />");
            sr.Append($"<b>Service:</b> URL Shortener");
            sr.Append("<br />");
            sr.Append("<br />");
            sr.Append($"<b>Message:</b> {request.Message}");

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("miansalman586@gmail.com");
            mail.To.Add("miansalman586@gmail.com");
            mail.Subject = request.Subject;
            mail.Body = sr.ToString();
            mail.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("miansalman586@gmail.com", "xdhl byef buhp gwxn"),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mail);

            var hasEmail = await _contactDbContext.Contact.Where(e => e.EmailAddress == request.EmailAddress).SingleOrDefaultAsync();
            if (hasEmail == null)
            {
                await _contactDbContext.Contact.AddAsync(new Contact
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    EmailAddress = request.EmailAddress,
                    CreatedDate = DateTime.Now
                });
                await _contactDbContext.SaveChangesAsync();
            }

            ViewData["Message"] = _langTransHelper.TransText("MessageSent");

            return View("ContactUs");
        }

        public IActionResult URLChecker()
        {
            return View();
        }

        public IActionResult Lifestyle()
        {
            return View();
        }

        [Route("10-Simple-Habits-to-Transform-Your-Daily-Routine")]
        public IActionResult Lifestyle1()
        {
            return View();
        }

        [Route("The-Ultimate-Guide-to-Balancing-Work-Life-and-Wellness")]
        public IActionResult Lifestyle2()
        {
            return View();
        }

        [Route("Stylish-Living:-Affordable-Home-Decor-Ideas-for-Every-Budget")]
        public IActionResult Lifestyle3()
        {
            return View();
        }

        [Route("Top-Travel-Destinations-for-a-Soulful-Escape")]
        public IActionResult Lifestyle4()
        {
            return View();
        }

        [Route("Self-Care-101-Practical-Tips-for-a-Happier-You")]
        public IActionResult Lifestyle5()
        {
            return View();
        }

        public IActionResult Technology()
        {
            return View();
        }

        [Route("the-role-of-Technology-in-Shaping-Our-Future")]
        public IActionResult Technology1()
        {
            return View();
        }

        [Route("The-Evolution-of-Artificial-Intelligence-From-Concept-to-Reality")]
        public IActionResult Technology2()
        {
            return View();
        }

        [Route("Cybersecurity-in-a-Digital-World-How-to-Stay-Safe-Online")]
        public IActionResult Technology3()
        {
            return View();
        }

        [Route("Revolutionizing-Industries-How-IoT-is-Transforming-Everyday-Life")]
        public IActionResult Technology4()
        {
            return View();
        }

        [Route("The-Ultimate-Guide-to-Building-Your-Own-Smart-Home-Ecosystem")]
        public IActionResult Technology5()
        {
            return View();
        }

        public IActionResult HealthWellness()
        {
            return View();
        }

        [Route("Mindful-Living-How-to-Cultivate-a-Healthier-Mindset-Every-Day")]
        public IActionResult HealthWellness1()
        {
            return View();
        }

        [Route("The-Power-of-Nutrition-Building-a-Diet-for-Longevity-and-Vitality")]
        public IActionResult HealthWellness2()
        {
            return View();
        }

        [Route("Fitness-for-Life-Simple-Workouts-to-Boost-Your-Energy-Levels")]
        public IActionResult HealthWellness3()
        {
            return View();
        }

        [Route("Stress-Management-101-Techniques-for-a-Balanced-and-Peaceful-Life")]
        public IActionResult HealthWellness4()
        {
            return View();
        }

        [Route("The-Importance-of-Sleep-Unlocking-the-Secrets-to-Restful-Nights")]
        public IActionResult HealthWellness5()
        {
            return View();
        }

        public IActionResult PersonalFinance()
        {
            return View();
        }

        [Route("Budgeting-Made-Simple-Easy-Tips-for-Managing-Your-Money")]
        public IActionResult PersonalFinance1()
        {
            return View();
        }

        [Route("Investing-for-Beginners-Building-Wealth-with-Confidence")]
        public IActionResult PersonalFinance2()
        {
            return View();
        }

        [Route("The-Power-of-Saving-How-to-Build-a-Solid-Emergency-Fund")]
        public IActionResult PersonalFinance3()
        {
            return View();
        }

        [Route("Understanding-Credit-How-to-Improve-Your-Score-and-Save-Money")]
        public IActionResult PersonalFinance4()
        {
            return View();
        }

        [Route("Smart-Ways-to-Pay-Off-Debt-Strategies-for-Financial-Freedom")]
        public IActionResult PersonalFinance5()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> URLChecker(ShortURLModel request)
        {
            string urlCode = string.Empty;
            ShortedURL? url = null;

            if (request.URL.StartsWith($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/c/"))
            {
                urlCode = request.URL.Replace($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/c/", string.Empty);

                url = await _appDbContext.ShortedURL.Where(e => e.CustomizedURL == urlCode).SingleOrDefaultAsync();
            }
            else if (request.URL.StartsWith($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/"))
            {
                urlCode = request.URL.Replace($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/", string.Empty);

                long urlId = ConvertBase36ToDecimal(urlCode);

                url = await _appDbContext.ShortedURL.Where(e => e.ID == urlId).SingleOrDefaultAsync();
            }

            if (url == null)
            {
                return NotFound(new { message = _langTransHelper.TransText("NotFoundURL") });
            }

            return Ok(new { url.LongURL, url.View, LastView = url.LastView.ToString(), CreatedDate = url.CreatedDate.ToString() });
        }
    }
}
