using System.ComponentModel;

namespace HARFileInvestigator
{
    internal sealed class HarTraceEntry
    {
        public int EntryIndex { get; init; }
        public DateTimeOffset StartedDateTime { get; init; }
        public string Method { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string Host { get; init; } = string.Empty;
        public int Status { get; init; }
        public string StatusText { get; init; } = string.Empty;
        public string MimeType { get; init; } = string.Empty;
        public double DurationMs { get; init; }
        public string IpAddress { get; init; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public Dictionary<string, string> AdditionalFields { get; init; } = [];

        [Browsable(false)]
        public string RequestText { get; init; } = string.Empty;

        [Browsable(false)]
        public string ResponseText { get; init; } = string.Empty;

        [Browsable(false)]
        public string SearchText { get; init; } = string.Empty;

        public string GetAdditionalField(string key)
        {
            return AdditionalFields.TryGetValue(key, out var value)
                ? value
                : string.Empty;
        }
    }
}
