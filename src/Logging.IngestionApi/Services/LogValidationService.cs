using Logging.Abstractions.Contracts;
using Logging.Abstractions.Interfaces;

public sealed class LogValidationService : ILogValidationService
{
    public List<string> Validate(LogEvent e)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(e.Message))
            errors.Add("message is required.");

        if (e.Timestamp == default)
            errors.Add("timestamp is required.");

        return errors;
    }

    public bool IsValid(LogEvent e, out string? reason)
    {
        var errors = Validate(e);
        reason = errors.FirstOrDefault();
        return errors.Count == 0;
    }
}
