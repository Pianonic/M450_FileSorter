namespace M450_FileSorter.Models
{
    public class RuleConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
        public string Target { get; set; } = string.Empty;
    }
}
