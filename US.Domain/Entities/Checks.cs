using US.Domain.Enums;

namespace US.Domain.Entities;

public class Check
{
    public Guid Id { get; private set; }
    public Guid MonitorId { get; private set; }
    public DateTimeOffset CheckedAt { get; private set; }
    public CheckStatus Status { get; private set; }
    public int? ResponseTimeMs { get; private set; }
    public int? StatusCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    public bool WasInMaintenance { get; private set; }

    /// <summary>
    /// Null, gdy check był poza oknem albo wystąpienie zostało usunięte (ON DELETE SET NULL) —
    /// wtedy <see cref="WasInMaintenance"/> nadal mówi prawdę.
    /// </summary>
    public Guid? MaintenanceOccurrenceId { get; private set; }

    private Check() { }

    public static Check Create(
        Guid monitorId,
        DateTimeOffset checkedAt,
        CheckStatus status,
        int? responseTimeMs = null,
        int? statusCode = null,
        string? errorMessage = null)
    {
        if (monitorId == Guid.Empty)
            throw new ArgumentException("MonitorId nie może być pusty.", nameof(monitorId));

        if (responseTimeMs is < 0)
            throw new ArgumentOutOfRangeException(nameof(responseTimeMs), "Czas odpowiedzi nie może być ujemny.");

        if (statusCode is not null and (< 100 or > 599))
            throw new ArgumentOutOfRangeException(nameof(statusCode), "Status code musi być w zakresie 100-599.");

        if (status == CheckStatus.DOWN && string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Nieudany check wymaga error_message.", nameof(errorMessage));

        if (status == CheckStatus.UP && errorMessage is not null)
            throw new ArgumentException("Udany check nie powinien mieć error_message.", nameof(errorMessage));

        return new Check
        {
            Id = Guid.NewGuid(),
            MonitorId = monitorId,
            CheckedAt = checkedAt,
            Status = status,
            ResponseTimeMs = responseTimeMs,
            StatusCode = statusCode,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Oznacza check jako wykonany w trakcie wystąpienia okna serwisowego.
    /// </summary>
    public void MarkInMaintenance(MaintenanceOccurrence occurrence)
    {
        if (!occurrence.IsActiveAt(CheckedAt))
            throw new InvalidOperationException("Check nie mieści się w tym wystąpieniu okna serwisowego.");

        WasInMaintenance = true;
        MaintenanceOccurrenceId = occurrence.Id;
    }
}
