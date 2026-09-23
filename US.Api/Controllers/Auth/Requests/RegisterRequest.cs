using System.ComponentModel.DataAnnotations;

namespace US.Api.Controllers.Auth.Requests;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    string? DisplayName);