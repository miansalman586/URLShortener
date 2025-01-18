using System.Net.Mail;
using System.Net;
using Google.Cloud.Translation.V2;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace URLShortener.Middlewares
{
    public class LanguageMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly TranslationClient client;

        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        public LanguageMiddleware(RequestDelegate next, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _next = next;

            _memoryCache = memoryCache;
            _configuration = configuration;

            client = TranslationClient.CreateFromApiKey(_configuration["GoogleTranslationAPIKey"]);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_memoryCache.TryGetValue("Langs", out IList<Language>? langs))
            {
                langs = await client.ListLanguagesAsync();

                _memoryCache.Set("Langs", langs);
            }

            if (langs.Any(e => "/" + e.Code == context.Request.Path.Value))
            {
                context.Response.Cookies.Append("Lang", context.Request.Path.Value.ToLower().Replace("/", string.Empty));
                context.Items["LangCode"] = context.Request.Path.Value.ToLower().Replace("/", string.Empty);
            }
            else
            {
                if (context.Request.Cookies["Lang"] == null || context.Request.Cookies["Lang"] == "en")
                {
                    context.Items["LangCode"] = "en";
                }
                else
                {
                    context.Items["LangCode"] = context.Request.Cookies["Lang"];
                }
            }

            context.Items["langs"] = langs;

            await _next(context);
        }
    }
}
