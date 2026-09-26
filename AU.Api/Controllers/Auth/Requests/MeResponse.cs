namespace AU.Api.Controllers.Auth.Requests;

public record MeResponse(Guid Id, string Email, string? DisplayName);