using YamlDotNet.Serialization;

namespace M450_FileSorter.Models
{
    public class Configuration
    {
        [YamlMember(Alias = "source_directories")]
        public List<string> SourceDirectories { get; set; } = new List<string>();

        public List<RuleConfig> Rules { get; set; } = new List<RuleConfig>();

        [YamlMember(Alias = "conflict_resolution")]
        public string ConflictResolution { get; set; } = "error";

        [YamlMember(Alias = "simulation_mode")]
        public bool SimulationMode { get; set; } = false;
    }
}
