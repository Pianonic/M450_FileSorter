using M450_FileSorter.Models;
using M450_FileSorter.Rules;

namespace M450_FileSorter.Tests.Rules
{
    public class RegexRuleTests
    {
        private readonly FileMetadata _invoiceFile = new("c:/docs/Rechnung_2023-12-05_AB123.pdf", "Rechnung_2023-12-05_AB123.pdf", 500, DateTime.UtcNow, DateTime.UtcNow);
        private readonly FileMetadata _imageFile = new("c:/pics/IMG_001.jpg", "IMG_001.jpg", 1000, DateTime.UtcNow, DateTime.UtcNow);
        private readonly FileMetadata _otherFile = new("c:/temp/data.log", "data.log", 100, DateTime.UtcNow, DateTime.UtcNow);

        [Fact]
        public void Matches_CorrectPattern_ReturnsTrue()
        {
            // Arrange
            var rule = new RegexRule("InvoiceRule", "/invoices/{$2}/{$3}", new Dictionary<string, string> { { "pattern", @"^(Rechnung|Invoice)_(\d{4})-(\d{2})-\d{2}.*\.pdf$" } });

            // Act & Assert
            Assert.True(rule.Matches(_invoiceFile));
        }

        [Fact]
        public void Matches_IncorrectPattern_ReturnsFalse()
        {
            // Arrange
            var rule = new RegexRule("InvoiceRule", "/invoices/{$2}/{$3}", new Dictionary<string, string> { { "pattern", @"^(Rechnung|Invoice)_(\d{4})-(\d{2})-\d{2}.*\.pdf$" } });

            // Act & Assert
            Assert.False(rule.Matches(_imageFile));
            Assert.False(rule.Matches(_otherFile));
        }

        [Fact]
        public void Matches_WithOptionalBasePattern_RequiresBothPatternsToMatch()
        {
            // Arrange
            // Hauptpattern: Invoice Format, Base Pattern: Nur PDFs
            var rule = new RegexRule("InvoicePdfRule", "/invoices/{$2}/{$3}", new Dictionary<string, string> {
                { "pattern", @"^(Rechnung|Invoice)_(\d{4})-(\d{2})-\d{2}.*$" }, // Hauptpattern (ohne .pdf)
                { "base_pattern", @"\.pdf$" } // Simuliertes optionales Pattern
            });

            Assert.True(rule.Matches(_invoiceFile));
        }


        [Fact]
        public void GetTargetDirectory_ReplacesCaptureGroups()
        {
            // Arrange
            var rule = new RegexRule("InvoiceRule", "/invoices/{$2}/{$3}", new Dictionary<string, string> { { "pattern", @"^(Rechnung|Invoice)_(\d{4})-(\d{2})-\d{2}.*\.pdf$" } });

            // Act
            var targetDir = rule.GetTargetDirectory(_invoiceFile);

            // Assert
            Assert.Equal("/invoices/2023/12", targetDir); // $2 = 2023, $3 = 12
        }

        [Fact]
        public void GetTargetDirectory_NoMatch_ReturnsOriginalTemplate()
        {
            // Arrange
            var rule = new RegexRule("InvoiceRule", "/invoices/{$2}/{$3}", new Dictionary<string, string> { { "pattern", @"^(Rechnung|Invoice)_(\d{4})-(\d{2})-\d{2}.*\.pdf$" } });

            // Act
            var targetDir = rule.GetTargetDirectory(_otherFile); // Passt nicht zum Pattern

            // Assert
            Assert.Equal("/invoices/{$2}/{$3}", targetDir); // Template bleibt unverändert
        }
    }
}
