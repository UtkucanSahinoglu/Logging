namespace Logging.IngestionApi.Configuration
{
    public sealed class ApiKeySettings
    {
        public const string SectionName = "ApiKey";

        public string HeaderName { get; set; } = "X-API-KEY";
        public string Value { get; set; } = string.Empty;
    }
}
