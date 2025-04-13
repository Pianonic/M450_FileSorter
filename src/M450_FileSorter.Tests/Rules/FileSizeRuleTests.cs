using M450_FileSorter.Models;
using M450_FileSorter.Rules;

namespace M450_FileSorter.Tests.Rules
{
    public class FileSizeRuleTests
    {
        [Fact]
        public void Matches_MinSizeMb_ReturnsTrueForLargerFile()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { "min_size_mb", "10" } };
            var rule = new FileSizeRule("LargeFiles", "/target/large", parameters);
            var largeFile = new FileMetadata("large.dat", "large.dat", 15 * 1024 * 1024, DateTime.UtcNow, DateTime.UtcNow); // 15MB

            // Act
            bool matches = rule.Matches(largeFile);

            // Assert
            Assert.True(matches);
        }
    }
}
