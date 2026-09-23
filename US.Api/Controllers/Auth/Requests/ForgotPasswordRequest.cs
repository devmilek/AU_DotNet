using System.ComponentModel.DataAnnotations;

namespace US.Api.Controllers.Auth.Requests;

public record ForgotPasswordRequest([Required, EmailAddress] string Email);