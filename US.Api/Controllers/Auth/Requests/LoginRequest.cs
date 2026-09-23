using System.ComponentModel.DataAnnotations;

namespace US.Api.Controllers.Auth.Requests;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    bool RememberMe = false);