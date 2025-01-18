using System.Net.Mail;
using System.Net;
using URLShortener.Data;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using URLShortener.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace URLShortener.Middlewares
{
    public class UserDataMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TaskService _taskService;

        public UserDataMiddleware(RequestDelegate next, TaskService taskService)
        {
            _next = next;
            _taskService = taskService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string userAgent = context.Request.Headers["User-Agent"].ToString();
            string? ipAddress = string.Empty;

            var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                ipAddress = forwardedHeader.Split(',').FirstOrDefault();
            }

            ipAddress = context.Connection.RemoteIpAddress?.ToString();

            _taskService.Execute(() =>
            {
                using (var scope = _taskService._serviceProvider.CreateScope())
                {
                    using (var appDbContext = scope.ServiceProvider.GetRequiredService<UserAgentDbContext>())
                    {
                        var dbUserAgent = appDbContext.UserAgent.Where(e => e.UserAgentString == userAgent).SingleOrDefault();
                        if (dbUserAgent == null)
                        {
                            appDbContext.UserAgent.Add(new Entity.UserAgent
                            {
                                UserAgentString = userAgent,
                                IPAddress = ipAddress,
                                CreatedDate = DateTime.Now
                            });

                            try
                            {
                                appDbContext.SaveChanges();
                            }
                            catch (DbUpdateException ex)
                            {
                                if (ex.InnerException?.Message.Contains("IX_UserAgent_UserAgent") == false)
                                {
                                    throw;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw;
                            }
                        }
                        else
                        {
                            dbUserAgent.IPAddress = ipAddress;
                            dbUserAgent.UpdatedDate = DateTime.Now;
                            appDbContext.UserAgent.Update(dbUserAgent);

                            appDbContext.SaveChanges();
                        }
                    }

                    using (var appDbContext = scope.ServiceProvider.GetRequiredService<IPAddressDbContext>())
                    {
                        var hasIP = appDbContext.IPAddress.Where(e => e.IPAddressString == ipAddress).SingleOrDefault();
                        if (hasIP == null)
                        {
                            appDbContext.IPAddress.Add(new Entity.IPAddress
                            {
                                IPAddressString = ipAddress,
                                CreatedDate = DateTime.Now
                            });

                            try
                            {
                                appDbContext.SaveChanges();
                            }
                            catch (DbUpdateException ex)
                            {
                                if (ex.InnerException?.Message.Contains("IX_IPAddress_IPAddress") == false)
                                {
                                    throw;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw;
                            }
                        }
                    }
                }
            });

            await _next(context);
        }
    }
}
