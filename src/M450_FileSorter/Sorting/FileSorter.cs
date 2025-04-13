using M450_FileSorter.Interfaces;

namespace M450_FileSorter.Sorting
{
    public class FileSorter : IFileSorter
    {
        private readonly IConfigurationReader _configReader;
        private readonly IRuleFactory _ruleFactory;
        private readonly IFileSystem _fileSystem;
        private readonly IConflictDetector _conflictDetector;

        public FileSorter(IConfigurationReader configReader, IRuleFactory ruleFactory, IFileSystem fileSystem, IConflictDetector conflictDetector)
        {
            _configReader = configReader;
            _ruleFactory = ruleFactory;
            _fileSystem = fileSystem;
            _conflictDetector = conflictDetector;
        }

        public void SortFiles(string configPath)
        {
            Console.WriteLine($"--- Starting File Sort using config: {configPath} ---");

            Models.Configuration config;
            try
            {
                config = _configReader.ReadConfiguration(configPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Fatal] Failed to load configuration: {ex.Message}");
                return;
            }

            Console.WriteLine($"Simulation Mode: {config.SimulationMode}");
            Console.WriteLine($"Conflict Resolution: {config.ConflictResolution}");

            var rules = _ruleFactory.CreateRules(config.Rules).ToList();
            if (!rules.Any())
            {
                Console.WriteLine("[Warning] No valid rules loaded. Nothing to do.");
                return;
            }

            var conflicts = _conflictDetector.DetectConflicts(rules).ToList();
            if (conflicts.Any())
            {
                Console.WriteLine("\n--- Potential Rule Conflicts Detected ---");
                foreach (var conflict in conflicts)
                {
                    Console.WriteLine($"[Conflict] Between '{conflict.RuleA.Name}' and '{conflict.RuleB.Name}': {conflict.Reason}");
                }
                Console.WriteLine("--- End Conflicts ---");

                if (config.ConflictResolution.Equals("error", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[Fatal] Stopping due to detected conflicts and 'error' resolution strategy.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("[Info] No obvious rule conflicts detected.");
            }

            Console.WriteLine("\n--- Processing Source Directories ---");
            foreach (var dir in config.SourceDirectories)
            {
                string absoluteSourceDir = Path.GetFullPath(dir);
                Console.WriteLine($"Processing: {absoluteSourceDir}");
                if (!_fileSystem.DirectoryExists(absoluteSourceDir))
                {
                    Console.WriteLine($"[Warning] Source directory not found: {absoluteSourceDir}. Skipping.");
                    continue;
                }

                var files = _fileSystem.GetFiles(absoluteSourceDir);
                foreach (var filePath in files)
                {
                    var metadata = _fileSystem.GetFileMetadata(filePath);
                    if (metadata == null) continue;

                    var matchingRules = rules.Where(r => r.Matches(metadata)).ToList();

                    if (matchingRules.Count == 0) continue;

                    IRule? ruleToApply = null;
                    if (matchingRules.Count == 1)
                    {
                        ruleToApply = matchingRules.First();
                    }
                    else
                    {
                        Console.WriteLine($"[Conflict Info] File '{metadata.FileName}' matches multiple rules: {string.Join(", ", matchingRules.Select(r => r.Name))}");
                        switch (config.ConflictResolution.ToLowerInvariant())
                        {
                            case "first_match":
                                ruleToApply = matchingRules.First();
                                Console.WriteLine($"  -> Applying first matching rule: '{ruleToApply.Name}'");
                                break;
                            case "error":
                                Console.WriteLine($"[Error] Multiple rules matched '{metadata.FileName}' and resolution is 'error'. Skipping file.");
                                continue;
                            case "skip":
                                Console.WriteLine($"  -> Skipping file due to multiple matches and 'skip' resolution.");
                                continue;
                            case "log":
                                ruleToApply = matchingRules.First();
                                Console.WriteLine($"  -> Applying first matching rule ('log' resolution): '{ruleToApply.Name}'");
                                break;
                            default:
                                Console.WriteLine($"[Warning] Unknown conflict resolution strategy '{config.ConflictResolution}'. Skipping file '{metadata.FileName}'.");
                                continue;
                        }
                    }

                    if (ruleToApply != null)
                    {
                        try
                        {
                            string targetDirRelative = ruleToApply.GetTargetDirectory(metadata);
                            string targetDirAbsolute = Path.GetFullPath(targetDirRelative);
                            string destinationPath = Path.Combine(targetDirAbsolute, metadata.FileName);

                            if (!config.SimulationMode)
                            {
                                _fileSystem.CreateDirectory(targetDirAbsolute);
                                _fileSystem.MoveFile(metadata.FullPath, destinationPath);
                            }
                            else
                            {
                                Console.WriteLine($"[Simulate] Move '{metadata.FileName}' from '{metadata.FullPath}' to '{destinationPath}' (Rule: '{ruleToApply.Name}')");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Error] Failed to process file '{metadata.FileName}' with rule '{ruleToApply.Name}': {ex.Message}");
                        }
                    }
                }
            }
            Console.WriteLine("--- File Sort Finished ---");
        }
    }
}
