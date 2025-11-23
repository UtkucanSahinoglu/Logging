using Logging.Abstractions.Contracts;

namespace Logging.Abstractions.Interfaces
{
    public interface ILogValidationService
    {
        /// <summary>
        /// Validate a single log. Returns a list of errors. Empty = valid.
        /// </summary>
        List<string> Validate(LogEvent e);

        /// <summary>
        /// Quick check: true/false + reason.
        /// </summary>
        bool IsValid(LogEvent e, out string? reason);
    }
}
