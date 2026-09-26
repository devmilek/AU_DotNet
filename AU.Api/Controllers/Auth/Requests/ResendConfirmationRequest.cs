using System.ComponentModel.DataAnnotations;

namespace AU.Api.Controllers.Auth.Requests;

public record ResendConfirmationRequest([Required, EmailAddress] string Email);