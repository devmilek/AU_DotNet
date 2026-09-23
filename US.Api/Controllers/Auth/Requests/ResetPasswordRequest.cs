using System.ComponentModel.DataAnnotations;

namespace US.Api.Controllers.Auth.Requests;

public record ResetPasswordRequest(
    [Required] Guid UserId,
    [Required] string Token,
    [Required] string NewPassword);