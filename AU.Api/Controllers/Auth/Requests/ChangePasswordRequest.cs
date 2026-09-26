using System.ComponentModel.DataAnnotations;

namespace AU.Api.Controllers.Auth.Requests;

public record ChangePasswordRequest([Required] string CurrentPassword, [Required] string NewPassword);
