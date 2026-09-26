using System.ComponentModel.DataAnnotations;

namespace AU.Api.Controllers.Auth.Requests;

public record ForgotPasswordRequest([Required, EmailAddress] string Email);