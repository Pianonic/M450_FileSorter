using M450_FileSorter.Interfaces;
using M450_FileSorter.Models;
using M450_FileSorter.Sorting;
using Moq;

namespace M450_FileSorter.Tests.Sorting
{
    public class FileSorterTests
    {
        private readonly Mock<IConfigurationReader> _mockConfigReader;
        private readonly Mock<IRuleFactory> _mockRuleFactory;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IConflictDetector> _mockConflictDetector;
        private readonly FileSorter _fileSorter;

        private readonly string _testConfigPath = "test_config.yaml";
        private readonly string _sourceDir = Path.GetFullPath("./TestSource");
        private readonly string _targetDirRule1 = Path.GetFullPath("./TestTarget/Rule1");
        private readonly string _targetDirRule2 = Path.GetFullPath("./TestTarget/Rule2");
        private readonly string _testFileName = "testfile.txt";
        private readonly string _testFilePath;
        private readonly FileMetadata _testFileMetadata;

        public FileSorterTests()
        {
            _mockConfigReader = new Mock<IConfigurationReader>();
            _mockRuleFactory = new Mock<IRuleFactory>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockConflictDetector = new Mock<IConflictDetector>();

            _fileSorter = new FileSorter(
                _mockConfigReader.Object,
                _mockRuleFactory.Object,
                _mockFileSystem.Object,
                _mockConflictDetector.Object
            );

            // Standard-Setup für Metadaten und Pfade
            _testFilePath = Path.Combine(_sourceDir, _testFileName);
            _testFileMetadata = new FileMetadata(_testFilePath, _testFileName, 1024, DateTime.UtcNow, DateTime.UtcNow);

            // Standard-Setup für Mocks
            _mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true); // Annahme: Verzeichnisse existieren
            _mockFileSystem.Setup(fs => fs.GetFiles(_sourceDir)).Returns(new List<string> { _testFilePath });
            _mockFileSystem.Setup(fs => fs.GetFileMetadata(_testFilePath)).Returns(_testFileMetadata);
            _mockConflictDetector.Setup(cd => cd.DetectConflicts(It.IsAny<IEnumerable<IRule>>())).Returns(Enumerable.Empty<RuleConflict>()); // Kein Konflikt standardmäßig
        }

        private Models.Configuration CreateTestConfig(string conflictResolution = "first_match", bool simulation = false)
        {
            return new Models.Configuration
            {
                SourceDirectories = new List<string> { _sourceDir },
                Rules = new List<RuleConfig>(), // Regeln werden über Factory gemockt
                ConflictResolution = conflictResolution,
                SimulationMode = simulation
            };
        }

        private Mock<IRule> CreateMockRule(string name, string targetTemplate, bool matches)
        {
            var mockRule = new Mock<IRule>();
            mockRule.SetupGet(r => r.Name).Returns(name);
            mockRule.SetupGet(r => r.TargetTemplate).Returns(targetTemplate); // Wichtig für Konflikterkennung
            mockRule.Setup(r => r.Matches(It.IsAny<FileMetadata>())).Returns(matches);
            // Setup GetTargetDirectory relativ zum targetTemplate
            mockRule.Setup(r => r.GetTargetDirectory(It.IsAny<FileMetadata>())).Returns(targetTemplate);
            return mockRule;
        }

        [Fact]
        public void SortFiles_SingleMatchingRule_MovesFile()
        {
            // Arrange
            var config = CreateTestConfig();
            _mockConfigReader.Setup(cr => cr.ReadConfiguration(_testConfigPath)).Returns(config);

            var rule1 = CreateMockRule("Rule1", _targetDirRule1, true);
            _mockRuleFactory.Setup(rf => rf.CreateRules(It.IsAny<IEnumerable<RuleConfig>>())).Returns(new List<IRule> { rule1.Object });

            string expectedDestinationPath = Path.Combine(_targetDirRule1, _testFileName);

            // Act
            _fileSorter.SortFiles(_testConfigPath);

            // Assert
            _mockFileSystem.Verify(fs => fs.CreateDirectory(_targetDirRule1), Times.Once);
            _mockFileSystem.Verify(fs => fs.MoveFile(_testFilePath, expectedDestinationPath), Times.Once);
        }

        [Fact]
        public void SortFiles_NoMatchingRule_DoesNotMoveFile()
        {
            // Arrange
            var config = CreateTestConfig();
            _mockConfigReader.Setup(cr => cr.ReadConfiguration(_testConfigPath)).Returns(config);

            var rule1 = CreateMockRule("Rule1", _targetDirRule1, false); // Passt nicht
            _mockRuleFactory.Setup(rf => rf.CreateRules(It.IsAny<IEnumerable<RuleConfig>>())).Returns(new List<IRule> { rule1.Object });

            // Act
            _fileSorter.SortFiles(_testConfigPath);

            // Assert
            _mockFileSystem.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(fs => fs.MoveFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void SortFiles_MultipleMatches_FirstMatchResolution_MovesToFirstRuleTarget()
        {
            // Arrange
            var config = CreateTestConfig(conflictResolution: "first_match");
            _mockConfigReader.Setup(cr => cr.ReadConfiguration(_testConfigPath)).Returns(config);

            var rule1 = CreateMockRule("Rule1", _targetDirRule1, true); // Passt, ist erster
            var rule2 = CreateMockRule("Rule2", _targetDirRule2, true); // Passt auch
            _mockRuleFactory.Setup(rf => rf.CreateRules(It.IsAny<IEnumerable<RuleConfig>>())).Returns(new List<IRule> { rule1.Object, rule2.Object });

            string expectedDestinationPath = Path.Combine(_targetDirRule1, _testFileName); // Ziel von Regel 1

            // Act
            _fileSorter.SortFiles(_testConfigPath);

            // Assert
            _mockFileSystem.Verify(fs => fs.CreateDirectory(_targetDirRule1), Times.Once);
            _mockFileSystem.Verify(fs => fs.MoveFile(_testFilePath, expectedDestinationPath), Times.Once);
            _mockFileSystem.Verify(fs => fs.CreateDirectory(_targetDirRule2), Times.Never); // Regel 2 wird nicht verwendet
        }

        [Fact]
        public void SortFiles_MultipleMatches_SkipResolution_DoesNotMoveFile()
        {
            // Arrange
            var config = CreateTestConfig(conflictResolution: "skip");
            _mockConfigReader.Setup(cr => cr.ReadConfiguration(_testConfigPath)).Returns(config);

            var rule1 = CreateMockRule("Rule1", _targetDirRule1, true);
            var rule2 = CreateMockRule("Rule2", _targetDirRule2, true);
            _mockRuleFactory.Setup(rf => rf.CreateRules(It.IsAny<IEnumerable<RuleConfig>>())).Returns(new List<IRule> { rule1.Object, rule2.Object });

            // Act
            _fileSorter.SortFiles(_testConfigPath);

            // Assert
            _mockFileSystem.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(fs => fs.MoveFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void SortFiles_MultipleMatches_LogErrorResolution_MovesToFirstRuleTarget()
        {
            // Arrange
            var config = CreateTestConfig(conflictResolution: "log"); // 'log' verhält sich wie 'first_match' nach dem Loggen
            _mockConfigReader.Setup(cr => cr.ReadConfiguration(_testConfigPath)).Returns(config);

            var rule1 = CreateMockRule("Rule1", _targetDirRule1, true); // Passt, ist erster
            var rule2 = CreateMockRule("Rule2", _targetDirRule2, true); // Passt auch
            _mockRuleFactory.Setup(rf => rf.CreateRules(It.IsAny<IEnumerable<RuleConfig>>())).Returns(new List<IRule> { rule1.Object, rule2.Object });

            string expectedDestinationPath = Path.Combine(_targetDirRule1, _testFileName); // Ziel von Regel 1

            // Act
            _fileSorter.SortFiles(_testConfigPath);

            // Assert
            // Überprüfen, ob geloggt wurde (schwierig ohne Logger-Mock), aber prüfen ob verschoben wurde
            _mockFileSystem.Verify(fs => fs.CreateDirectory(_targetDirRule1), Times.Once);
            _mockFileSystem.Verify(fs => fs.MoveFile(_testFilePath, expectedDestinationPath), Times.Once);
            _mockFileSystem.Verify(fs => fs.CreateDirectory(_targetDirRule2), Times.Never);
        }


        [Fact]
        public void SortFiles_ConflictDetected_ErrorResolution_StopsProcessing()
        {
            // Arrange
            var config = CreateTestConfig(conflictResolution: "error");
            _mockConfigReader.Setup(cr => cr.ReadConfiguration(_testConfigPath)).Returns(config);

            var rule1 = CreateMockRule("Rule1", _targetDirRule1, true);
            var rule2 = CreateMockRule("Rule2", _targetDirRule1, true); // Gleiches Ziel -> Konflikt
            var rules = new List<IRule> { rule1.Object, rule2.Object };
            _mockRuleFactory.Setup(rf => rf.CreateRules(It.IsAny<IEnumerable<RuleConfig>>())).Returns(rules);

            // Konflikt melden
            var conflict = new RuleConflict(rule1.Object, rule2.Object, "Same target");
            _mockConflictDetector.Setup(cd => cd.DetectConflicts(rules)).Returns(new List<RuleConflict> { conflict });

            // Act
            _fileSorter.SortFiles(_testConfigPath);

            // Assert
            // Es darf gar nicht erst versucht werden, Dateien zu lesen oder zu verschieben
            _mockFileSystem.Verify(fs => fs.GetFiles(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(fs => fs.GetFileMetadata(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(fs => fs.MoveFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void SortFiles_SimulationMode_DoesNotCreateOrMove()
        {
            // Arrange
            var config = CreateTestConfig(simulation: true); // Simulation an
            _mockConfigReader.Setup(cr => cr.ReadConfiguration(_testConfigPath)).Returns(config);

            var rule1 = CreateMockRule("Rule1", _targetDirRule1, true);
            _mockRuleFactory.Setup(rf => rf.CreateRules(It.IsAny<IEnumerable<RuleConfig>>())).Returns(new List<IRule> { rule1.Object });

            // Act
            _fileSorter.SortFiles(_testConfigPath);

            // Assert
            _mockFileSystem.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(fs => fs.MoveFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void SortFiles_SourceDirectoryNotFound_SkipsDirectory()
        {
            // Arrange
            var config = CreateTestConfig();
            string nonExistentDir = Path.GetFullPath("./NonExistentSource");
            config.SourceDirectories = new List<string> { nonExistentDir }; // Nur das nicht existente Verzeichnis
            _mockConfigReader.Setup(cr => cr.ReadConfiguration(_testConfigPath)).Returns(config);

            var rule1 = CreateMockRule("Rule1", _targetDirRule1, true);
            _mockRuleFactory.Setup(rf => rf.CreateRules(It.IsAny<IEnumerable<RuleConfig>>())).Returns(new List<IRule> { rule1.Object });

            // Setup: Verzeichnis existiert nicht
            _mockFileSystem.Setup(fs => fs.DirectoryExists(nonExistentDir)).Returns(false);

            // Act
            _fileSorter.SortFiles(_testConfigPath);

            // Assert
            // Darf nicht versuchen, Dateien aus dem nicht existenten Verzeichnis zu lesen
            _mockFileSystem.Verify(fs => fs.GetFiles(nonExistentDir), Times.Never);
            _mockFileSystem.Verify(fs => fs.MoveFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void SortFiles_GetFileMetadataReturnsNull_SkipsFile()
        {
            // Arrange
            var config = CreateTestConfig();
            _mockConfigReader.Setup(cr => cr.ReadConfiguration(_testConfigPath)).Returns(config);

            var rule1 = CreateMockRule("Rule1", _targetDirRule1, true);
            _mockRuleFactory.Setup(rf => rf.CreateRules(It.IsAny<IEnumerable<RuleConfig>>())).Returns(new List<IRule> { rule1.Object });

            // Setup: GetFileMetadata gibt null zurück
            _mockFileSystem.Setup(fs => fs.GetFileMetadata(_testFilePath)).Returns((FileMetadata?)null);

            // Act
            _fileSorter.SortFiles(_testConfigPath);

            // Assert
            // Darf nicht versuchen, die Datei zu verschieben
            _mockFileSystem.Verify(fs => fs.MoveFile(_testFilePath, It.IsAny<string>()), Times.Never);
        }
    }
}
