using System.ComponentModel.DataAnnotations;

namespace US.Api.Controllers.Auth.Requests;

public record ConfirmEmailRequest(
    [Required] Guid UserId,
    [Required] string Token);