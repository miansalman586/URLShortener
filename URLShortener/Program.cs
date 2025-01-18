using Microsoft.EntityFrameworkCore;
using URLShortener.Data;
using URLShortener.Helpers;
using URLShortener.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<UserAgentDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("UserAgentConnection")));
builder.Services.AddDbContext<IPAddressDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("IPAddressConnection")));
builder.Services.AddDbContext<ContactDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ContactConnection")));

builder.Services.AddSingleton<LangTransHelper>();
builder.Services.AddSingleton<TaskService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<UserDataMiddleware>();
app.UseMiddleware<LanguageMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
