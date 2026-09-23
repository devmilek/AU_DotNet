namespace US.Application.Exceptions;

/// <summary>
/// Operacja jest sprzeczna z aktualnym stanem zasobu. Mapowane w API na 409.
/// </summary>
public sealed class ConflictException(string message) : Exception(message);
