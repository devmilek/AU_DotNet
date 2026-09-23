using System.ComponentModel.DataAnnotations;

namespace US.Api.Controllers.Auth.Requests;

public record ResendConfirmationRequest([Required, EmailAddress] string Email);