using System.ComponentModel.DataAnnotations;

namespace AU.Api.Controllers.Auth.Requests;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    string? DisplayName);