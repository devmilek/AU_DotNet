using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using AU.Api.Controllers.Auth.Requests;
using AU.Api.RateLimiting;
using AU.Application.Notifications.Events.EmailConfirmationRequested;
using AU.Application.Notifications.Events.PasswordResetRequested;
using AU.Infrastructure.Identity;
using Wolverine;

namespace AU.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IWebHostEnvironment env, IMessageBus bus, IOptions<PasswordResetTokenProviderOptions> passwordResetOptions, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };
        
        var result = await userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
                return Accepted();

            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);
            return ValidationProblem(ModelState);
        }
        
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        if (env.IsDevelopment())
        {
            logger.LogInformation(
                "Confirm email: {Url}/confirm-email?userId={UserId}&token={Token}",
                configuration["Auth:FrontendUrl"], user.Id, encodedToken);
        }

        await SendConfirmationEmailAsync(user);
        return Accepted();
    }
    
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is not null && !user.EmailConfirmed)
            await SendConfirmationEmailAsync(user);

        return Accepted();
    }

    [EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            return BadRequest();

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch (FormatException)
        {
            return BadRequest();
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? NoContent() : BadRequest();
    }
    
    [EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return Unauthorized();

        var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent: request.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded) return NoContent();

        if (result.IsLockedOut) return Problem(statusCode: StatusCodes.Status423Locked, title: "Konto tymczasowo zablokowane. Spróbuj później.");

        if (result.IsNotAllowed) return Problem(statusCode: StatusCodes.Status403Forbidden, title: "Potwierdź adres email przed zalogowaniem.");

        return Unauthorized();
    }
    
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is not null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{configuration["Auth:FrontendUrl"]}/reset-password?userId={user.Id}&token={encodedToken}";

            await bus.PublishAsync(new PasswordResetRequestedEvent(
                user.Email!, user.DisplayName, url, (int)passwordResetOptions.Value.TokenLifespan.TotalMinutes));
        }

        return Accepted();
    }
    
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            return InvalidResetLink();

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch (FormatException)
        {
            return InvalidResetLink();
        }

        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code == "InvalidToken"))
                return InvalidResetLink();

            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);
            return ValidationProblem(ModelState);
        }

        // link z maila dowodzi dostępu do skrzynki
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        // zniesienie blokady po udanym resecie
        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);
        
        // todo: wysłać maila że hasło zostało zmienione
        // await bus.PublishAsync(new PasswordChangedEvent(user.Email!, user.DisplayName));

        return NoContent();
    }
    
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Unauthorized();

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);
            return ValidationProblem(ModelState);
        }

        await signInManager.RefreshSignInAsync(user);
        // todo: wysłać maila że hasło zostało zmienione
        //await bus.PublishAsync(new PasswordChangedEvent(user.Email!, user.DisplayName));

        return NoContent();
    }

    private IActionResult InvalidResetLink() =>
        Problem(statusCode: StatusCodes.Status400BadRequest,
            title: "Link resetujący jest nieprawidłowy lub wygasł.");

    
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("me")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<MeResponse>> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Unauthorized();

        return new MeResponse(user.Id, user.Email!, user.DisplayName);
    }
    
    private async Task SendConfirmationEmailAsync(ApplicationUser user)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var url = $"{configuration["Auth:FrontendUrl"]}/confirm-email?userId={user.Id}&token={encodedToken}";

        await bus.PublishAsync(new EmailConfirmationRequestedEvent(user.Email!, user.DisplayName, url));
    }
}