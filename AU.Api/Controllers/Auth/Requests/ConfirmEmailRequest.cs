using System.ComponentModel.DataAnnotations;

namespace AU.Api.Controllers.Auth.Requests;

public record ConfirmEmailRequest(
    [Required] Guid UserId,
    [Required] string Token);