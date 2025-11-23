namespace Logging.IngestionApi.Configuration
{
    public sealed class ElasticsearchSettings
    {
        public const string SectionName = "Elasticsearch";

        public string Url { get; set; } = default!;
        public string? Username { get; set; }
        public string? Password { get; set; }

        public bool BootstrapOnStartup { get; set; } = true;

        public int NumberOfShards { get; set; } = 1;
        public int NumberOfReplicas { get; set; } = 1;
    }
}
