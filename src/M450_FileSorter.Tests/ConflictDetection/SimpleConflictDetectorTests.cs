using M450_FileSorter.ConflictDetection;
using M450_FileSorter.Interfaces;
using Moq;

namespace M450_FileSorter.Tests.ConflictDetection
{
    public class SimpleConflictDetectorTests
    {
        private readonly SimpleConflictDetector _detector = new SimpleConflictDetector();

        [Fact]
        public void DetectConflicts_NoConflicts_ReturnsEmptyList()
        {
            // Arrange
            var rule1 = new Mock<IRule>();
            rule1.SetupGet(r => r.Name).Returns("Rule1");
            rule1.SetupGet(r => r.TargetTemplate).Returns("/target/A");

            var rule2 = new Mock<IRule>();
            rule2.SetupGet(r => r.Name).Returns("Rule2");
            rule2.SetupGet(r => r.TargetTemplate).Returns("/target/B");

            var rules = new List<IRule> { rule1.Object, rule2.Object };

            // Act
            var conflicts = _detector.DetectConflicts(rules);

            // Assert
            Assert.Empty(conflicts);
        }

        [Fact]
        public void DetectConflicts_IdenticalTargetTemplate_ReturnsConflict()
        {
            // Arrange
            var rule1 = new Mock<IRule>();
            rule1.SetupGet(r => r.Name).Returns("Rule1");
            rule1.SetupGet(r => r.TargetTemplate).Returns("/target/Same");

            var rule2 = new Mock<IRule>();
            rule2.SetupGet(r => r.Name).Returns("Rule2");
            rule2.SetupGet(r => r.TargetTemplate).Returns("/target/Same"); // Gleiches Ziel

            var rule3 = new Mock<IRule>();
            rule3.SetupGet(r => r.Name).Returns("Rule3");
            rule3.SetupGet(r => r.TargetTemplate).Returns("/target/Different");

            var rules = new List<IRule> { rule1.Object, rule2.Object, rule3.Object };

            // Act
            var conflicts = _detector.DetectConflicts(rules).ToList();

            // Assert
            Assert.Single(conflicts);
            var conflict = conflicts.First();
            Assert.Equal(rule1.Object, conflict.RuleA);
            Assert.Equal(rule2.Object, conflict.RuleB);
            Assert.Contains("same target template", conflict.Reason);
            Assert.Contains("/target/Same", conflict.Reason);
        }

        [Fact]
        public void DetectConflicts_IdenticalTargetTemplateCaseInsensitive_ReturnsConflict()
        {
            // Arrange
            var rule1 = new Mock<IRule>();
            rule1.SetupGet(r => r.Name).Returns("RuleUpper");
            rule1.SetupGet(r => r.TargetTemplate).Returns("/TARGET/CASE");

            var rule2 = new Mock<IRule>();
            rule2.SetupGet(r => r.Name).Returns("RuleLower");
            rule2.SetupGet(r => r.TargetTemplate).Returns("/target/case"); // Gleiches Ziel, andere Gross-/Kleinschreibung

            var rules = new List<IRule> { rule1.Object, rule2.Object };

            // Act
            var conflicts = _detector.DetectConflicts(rules).ToList();

            // Assert
            Assert.Single(conflicts);
            Assert.Equal(rule1.Object, conflicts[0].RuleA);
            Assert.Equal(rule2.Object, conflicts[0].RuleB);
        }
    }
}
