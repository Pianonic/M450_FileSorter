using M450_FileSorter.Factories;
using M450_FileSorter.Models;
using M450_FileSorter.Rules;

namespace M450_FileSorter.Tests.Factories
{
    public class RuleFactoryTests
    {
        private readonly RuleFactory _factory = new RuleFactory();

        [Fact]
        public void CreateRules_ValidConfigs_CreatesCorrectRuleTypes()
        {
            // Arrange
            var configs = new List<RuleConfig>
            {
                new RuleConfig { Name = "CDate", Type = "creation_date", Target = "t1" },
                new RuleConfig { Name = "LMod", Type = "last_modified_date", Target = "t2" },
                new RuleConfig { Name = "Size", Type = "file_size", Target = "t3" },
                new RuleConfig { Name = "Regex", Type = "regex", Target = "t4", Params = new Dictionary<string, string>{{"pattern", ".*"}} }
            };

            // Act
            var rules = _factory.CreateRules(configs).ToList();

            // Assert
            Assert.Equal(4, rules.Count);
            Assert.IsType<CreationDateRule>(rules[0]);
            Assert.Equal("CDate", rules[0].Name);
            Assert.IsType<LastModifiedDateRule>(rules[1]);
            Assert.Equal("LMod", rules[1].Name);
            Assert.IsType<FileSizeRule>(rules[2]);
            Assert.Equal("Size", rules[2].Name);
            Assert.IsType<RegexRule>(rules[3]);
            Assert.Equal("Regex", rules[3].Name);
        }

        [Fact]
        public void CreateRules_UnknownType_SkipsRule()
        {
            // Arrange
            var configs = new List<RuleConfig>
            {
                new RuleConfig { Name = "Known", Type = "creation_date", Target = "t1" },
                new RuleConfig { Name = "Unknown", Type = "unknown_type", Target = "t2" }
            };

            // Act
            var rules = _factory.CreateRules(configs).ToList();

            // Assert
            Assert.Single(rules); // Nur die bekannte Regel sollte erstellt werden
            Assert.IsType<CreationDateRule>(rules[0]);
            Assert.Equal("Known", rules[0].Name);
        }

        [Fact]
        public void CreateRules_RegexRuleMissingPattern_SkipsRule()
        {
            // Arrange
            var configs = new List<RuleConfig>
            {
                new RuleConfig { Name = "BadRegex", Type = "regex", Target = "t1" } // Fehlendes 'pattern'
            };

            // Act
            var rules = _factory.CreateRules(configs).ToList();

            // Assert
            Assert.Empty(rules);
        }

        [Fact]
        public void CreateRules_RegexRuleInvalidPattern_SkipsRule()
        {
            // Arrange
            var configs = new List<RuleConfig>
            {
                new RuleConfig { Name = "InvalidRegex", Type = "regex", Target = "t1", Params = new Dictionary<string, string>{{"pattern", "["}} } // Ungültiges Regex
            };

            // Act
            var rules = _factory.CreateRules(configs).ToList();

            // Assert
            Assert.Empty(rules);
        }
    }
}
