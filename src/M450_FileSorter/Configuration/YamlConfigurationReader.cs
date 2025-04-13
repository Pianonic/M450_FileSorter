using M450_FileSorter.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace M450_FileSorter.Configuration
{
    public class YamlConfigurationReader : IConfigurationReader
    {
        public Models.Configuration ReadConfiguration(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Configuration file not found: {path}");
            }

            var yamlContent = File.ReadAllText(path);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            try
            {
                return deserializer.Deserialize<Models.Configuration>(yamlContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error parsing YAML configuration: {ex.Message}", ex);
            }
        }
    }
}
