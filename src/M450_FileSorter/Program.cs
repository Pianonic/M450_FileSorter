using M450_FileSorter.Configuration;
using M450_FileSorter.ConflictDetection;
using M450_FileSorter.Factories;
using M450_FileSorter.FileSystem;
using M450_FileSorter.Interfaces;
using M450_FileSorter.Sorting;

public class Program
{
    public static void Main(string[] args)
    {
        string configPath = args.Length > 0 ? args[0] : "config.yaml";
        configPath = Path.GetFullPath(configPath);

        Console.WriteLine($"Using configuration file: {configPath}");

        if (!File.Exists(configPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Fatal] Configuration file not found: {configPath}");
            Console.ResetColor();
            return;
        }

        // Dependency Injection
        IConfigurationReader configReader = new YamlConfigurationReader();
        IFileSystem fileSystem = new DefaultFileSystem();
        IRuleFactory ruleFactory = new RuleFactory();
        IConflictDetector conflictDetector = new SimpleConflictDetector();
        IFileSorter fileSorter = new FileSorter(
            configReader,
            ruleFactory,
            fileSystem,
            conflictDetector
        );

        try
        {
            fileSorter.SortFiles(configPath);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Operation completed.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Critical] An unexpected error occurred during sorting:");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }

        Thread.Sleep(5000);
    }
}