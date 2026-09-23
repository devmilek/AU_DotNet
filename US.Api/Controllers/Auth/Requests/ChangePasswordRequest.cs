using System.ComponentModel.DataAnnotations;

namespace US.Api.Controllers.Auth.Requests;

public record ChangePasswordRequest([Required] string CurrentPassword, [Required] string NewPassword);
