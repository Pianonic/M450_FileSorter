using M450_FileSorter.Configuration;

namespace M450_FileSorter.Tests.Configuration
{
    public class YamlConfigurationReaderTests
    {
        private readonly string _testDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");

        [Fact]
        public void ReadConfiguration_ValidFile_ReturnsConfiguration()
        {
            // Arrange
            var reader = new YamlConfigurationReader();
            string validConfigPath = Path.Combine(_testDataDir, "valid_config.yaml");

            // Act
            var config = reader.ReadConfiguration(validConfigPath);

            // Assert
            Assert.NotNull(config);
            Assert.Equal(2, config.SourceDirectories.Count);
            Assert.Equal("./Source1", config.SourceDirectories[0]);
            Assert.Equal("/absolute/path/Source2", config.SourceDirectories[1]);
            Assert.Equal(2, config.Rules.Count);
            Assert.Equal("Rule1", config.Rules[0].Name);
            Assert.Equal("creation_date", config.Rules[0].Type);
            Assert.Equal("./Target/TextFiles/{yyyy}/", config.Rules[0].Target);
            Assert.Equal("Rule2", config.Rules[1].Name);
            Assert.Equal("first_match", config.ConflictResolution);
            Assert.False(config.SimulationMode);
        }

        [Fact]
        public void ReadConfiguration_NonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var reader = new YamlConfigurationReader();
            string invalidPath = Path.Combine(_testDataDir, "non_existent_config.yaml");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => reader.ReadConfiguration(invalidPath));
        }

        [Fact]
        public void ReadConfiguration_InvalidYamlSyntax_ThrowsInvalidOperationException()
        {
            // Arrange
            var reader = new YamlConfigurationReader();
            string invalidSyntaxPath = Path.Combine(_testDataDir, "invalid_syntax.yaml");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => reader.ReadConfiguration(invalidSyntaxPath));
        }
    }
}
