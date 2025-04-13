using M450_FileSorter.Models;
using M450_FileSorter.Rules;

namespace M450_FileSorter.Tests.Rules
{
    public class CreationDateRuleTests
    {
        private readonly FileMetadata _testFileTxt = new("c:/test.txt", "test.txt", 100, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-5));
        private readonly FileMetadata _testFileJpg = new("c:/image.jpg", "image.jpg", 2000, DateTime.UtcNow.AddYears(-1), DateTime.UtcNow.AddMonths(-1));

        [Fact]
        public void Matches_NoPattern_ReturnsTrue()
        {
            // Arrange
            var rule = new CreationDateRule("Test", "/target", new Dictionary<string, string>());

            // Act & Assert
            Assert.True(rule.Matches(_testFileTxt));
            Assert.True(rule.Matches(_testFileJpg));
        }

        [Fact]
        public void Matches_WithPattern_ReturnsCorrectly()
        {
            // Arrange
            var rule = new CreationDateRule("Test", "/target", new Dictionary<string, string> { { "pattern", @"\.txt$" } });

            // Act & Assert
            Assert.True(rule.Matches(_testFileTxt)); // Endet mit .txt
            Assert.False(rule.Matches(_testFileJpg)); // Endet nicht mit .txt
        }

        [Fact]
        public void Matches_WithInvalidPatternInParams_IgnoresPatternAndReturnsTrue()
        {
            // Arrange
            // Die Factory würde dies normalerweise abfangen, aber wir testen die Regel isoliert
            var rule = new CreationDateRule("Test", "/target", new Dictionary<string, string> { { "pattern", @"[" } }); // Ungültiges Regex

            // Act & Assert
            Assert.True(rule.Matches(_testFileTxt)); // Sollte true zurückgeben, da das Pattern ignoriert wird
            Assert.True(rule.Matches(_testFileJpg));
        }

        [Fact]
        public void GetTargetDirectory_ReplacesDatePlaceholders()
        {
            // Arrange
            var rule = new CreationDateRule("Test", "/archive/{yyyy}/{MM}/{dd}", new Dictionary<string, string>());
            var fileDate = new DateTime(2023, 10, 26, 10, 0, 0, DateTimeKind.Utc);
            var fileMeta = new FileMetadata("path", "file.dat", 1, fileDate, fileDate);

            // Act
            var targetDir = rule.GetTargetDirectory(fileMeta);

            // Assert
            Assert.Equal("/archive/2023/10/26", targetDir);
        }
    }
}
