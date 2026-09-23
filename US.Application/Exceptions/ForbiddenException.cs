namespace US.Application.Exceptions;

/// <summary>
/// Wywołujący jest uwierzytelniony, ale nie ma prawa do tej operacji.
/// Mapowane w API na 403.
/// </summary>
public sealed class ForbiddenException(string message = "You are not allowed to perform this action.")
    : Exception(message);
