using Google.Cloud.Translation.V2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using URLShortener.Data;

namespace URLShortener.Helpers
{
    public class LangTransHelper
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly TranslationClient client;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;

        public LangTransHelper(IHttpContextAccessor contextAccessor, IConfiguration configuration, IMemoryCache memoryCache, IServiceProvider serviceProvider)
        {
            _contextAccessor = contextAccessor;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;

            client = TranslationClient.CreateFromApiKey(_configuration["GoogleTranslationAPIKey"]);
        }

        public string TransText(string transText)
        {
            string langCode = _contextAccessor.HttpContext.Items["LangCode"].ToString();

            if (!_memoryCache.TryGetValue("TransText-" + langCode + "-" + transText, out string? text))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    using (var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>())
                    {
                        var Ttext = appDbContext.LangTrans.Where(e => e.LangCode == "en" && e.TextCode == transText).Single();
                        if (langCode == "en" && Ttext.Translation == null)
                        {
                            var transResult = client.TranslateText(Ttext.Text, langCode);
                            string tranText = transResult.TranslatedText;

                            text = tranText;

                            appDbContext.LangTrans.Update(Ttext);
                            appDbContext.SaveChanges();
                        }
                        else if (langCode != "en")
                        {
                            var TTtext = appDbContext.LangTrans.Where(e => e.LangCode == langCode && e.TextCode == transText).SingleOrDefault();
                            if (TTtext == null)
                            {
                                var transResult = client.TranslateText(Ttext.Text, langCode);
                                string tranText = transResult.TranslatedText;

                                TTtext = new Entity.LangTrans
                                {
                                    LangCode = langCode,
                                    TextCode = transText,
                                    Text = Ttext.Text,
                                    Translation = tranText
                                };

                                text = TTtext.Translation;

                                appDbContext.LangTrans.Add(TTtext);
                                appDbContext.SaveChanges();
                            }
                            else
                            {
                                text = TTtext.Translation;
                            }
                        }
                        else
                        {
                            text = Ttext.Translation;
                        }

                        _memoryCache.Set("TransText-" + langCode + "-" + transText, text);
                    }
                }
            }

            return text;
        }
    }
}
