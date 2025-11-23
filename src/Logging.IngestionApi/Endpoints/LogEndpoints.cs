using Logging.Abstractions.Contracts;
using Logging.Abstractions.Interfaces;
using Logging.IngestionApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Logging.IngestionApi.Endpoints
{
    public static class LogEndpoints
    {
        public static IEndpointRouteBuilder MapLogEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/logs")
                           .WithTags("Logs");

            // Single log ingest
            group.MapPost("/", HandleSingleAsync)
                 .WithName("IngestSingleLog")
                 .Produces(StatusCodes.Status202Accepted)
                 .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);

            // Bulk ingest
            group.MapPost("/batch", HandleBatchAsync)
                 .WithName("IngestBulkLogs")
                 .Produces(StatusCodes.Status202Accepted)
                 .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);

            // Ping endpoint (monitoring)
            group.MapGet("/ping", () => Results.Ok("Logs API OK"));

            return app;
        }

        private static async Task<Results<Accepted, BadRequest<ValidationErrorResponse>>> HandleSingleAsync(
            [FromBody] LogEvent logEvent,
            ILogWriter writer,
            ILogValidationService validator,
            CancellationToken ct)
        {
            var errors = validator.Validate(logEvent);

            if (errors.Count > 0)
            {
                return TypedResults.BadRequest(new ValidationErrorResponse { Errors = errors });
            }

            await writer.WriteAsync(logEvent, ct);
            return TypedResults.Accepted((string?)null);
        }

        private static async Task<Results<Accepted, BadRequest<ValidationErrorResponse>>> HandleBatchAsync(
            [FromBody] LogBatch batch,
            ILogWriter writer,
            ILogValidationService validator,
            CancellationToken ct)
        {
            if (batch.Events == null || batch.Events.Count == 0)
            {
                return TypedResults.BadRequest(new ValidationErrorResponse
                {
                    Errors = new List<string> { "events list must contain at least 1 item." }
                });
            }

            var allErrors = new List<string>();

            foreach (var evt in batch.Events)
            {
                var errors = validator.Validate(evt);
                if (errors.Count > 0)
                    allErrors.AddRange(errors);
            }

            if (allErrors.Count > 0)
            {
                return TypedResults.BadRequest(new ValidationErrorResponse
                {
                    Errors = allErrors
                });
            }

            await writer.WriteBatchAsync(batch.Events, ct);
            return TypedResults.Accepted((string?)null);
        }


        // ----------------------------------------------------------------------
        //  Validation Model
        // ----------------------------------------------------------------------
        public sealed class ValidationErrorResponse
        {
            public List<string> Errors { get; set; } = new();
        }
    }
}
