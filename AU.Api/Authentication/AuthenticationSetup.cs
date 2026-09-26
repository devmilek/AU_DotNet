using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using AU.Api.Authorization;
using AU.Application.Abstractions;
using AU.Application.Organizations;
using AU.Domain.Enums;
using AU.Infrastructure;
using AU.Infrastructure.Persistence;

namespace AU.Api.Authentication;

public static class AuthenticationSetup
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityPersistence().AddSignInManager();

        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "us_session";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Domain = configuration["Auth:CookieDomain"];

            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;

            options.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.Configure<SecurityStampValidatorOptions>(o =>
            o.ValidationInterval = TimeSpan.FromMinutes(5));

        services.AddAppDataProtection();

        
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(OrgPolicies.Member, p => p
                .RequireAuthenticatedUser()
                .AddRequirements(new OrgRoleRequirement(OrganizationRole.Member)))
            .AddPolicy(OrgPolicies.Admin, p => p
                .RequireAuthenticatedUser()
                .AddRequirements(new OrgRoleRequirement(OrganizationRole.Admin)))
            .AddPolicy(OrgPolicies.Owner, p => p
                .RequireAuthenticatedUser()
                .AddRequirements(new OrgRoleRequirement(OrganizationRole.Owner)));

        services.AddCors(o => o.AddPolicy("frontend", p => p
            .WithOrigins(configuration["Auth:FrontendUrl"]!)
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod()));
        
        services.Configure<InvitationOptions>(configuration.GetSection(InvitationOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAuthorizationHandler, OrgRoleHandler>();

        return services;

    }
}