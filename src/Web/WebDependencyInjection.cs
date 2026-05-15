using System.Threading.RateLimiting;
using Application.Common.Interfaces.Auth;
using Microsoft.AspNetCore.HttpOverrides;
using Presentation.Constants;
using Web.Constants;
using Web.Services;

namespace Web;

public static class WebDependencyInjection
{
    public static void AddWebDependencyInjection(this IServiceCollection services)
    {
        // CORS policy
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyConstants.LocalPolicy, b => b
                .WithOrigins(CorsPolicyConstants.LocalAllowedUrls)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );
            options.AddPolicy(CorsPolicyConstants.ProdPolicy, b => b
                .WithOrigins(CorsPolicyConstants.ProdAllowedUrls)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );
        });

        // Rate limiting
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimiterConstants.AnonymousUserPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = RateLimiterConstants.AnonymousUserPermitLimit,
                        Window = TimeSpan.FromSeconds(RateLimiterConstants.AnonymousUserWindowSeconds),
                    }));
        });

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        services.AddScoped<IUser, CurrentUser>();
        services.AddHttpContextAccessor();

        services.AddEndpointsApiExplorer();
    }
}
