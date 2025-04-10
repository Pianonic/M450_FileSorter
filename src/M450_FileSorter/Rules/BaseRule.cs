using M450_FileSorter.Interfaces;
using M450_FileSorter.Models;
using System.Text.RegularExpressions;

namespace M450_FileSorter.Rules
{
    public abstract class BaseRule : IRule
    {
        public string Name { get; }
        public string TargetTemplate { get; }
        protected Regex? PatternRegex { get; }

        protected BaseRule(string name, string target, Dictionary<string, string> parameters)
        {
            Name = name;
            TargetTemplate = target;

            if (parameters.TryGetValue("pattern", out var pattern) && !string.IsNullOrWhiteSpace(pattern))
            {
                try
                {
                    PatternRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"[Warning] Invalid regex pattern for rule '{Name}': {pattern}. Pattern ignored. Error: {ex.Message}");
                    PatternRegex = null;
                }
            }
        }

        protected bool MatchesPattern(string fileName)
        {
            return PatternRegex == null || PatternRegex.IsMatch(fileName);
        }

        public abstract bool Matches(FileMetadata fileMetadata);
        public abstract string GetTargetDirectory(FileMetadata fileMetadata);

        protected string ReplaceDatePlaceholders(string template, DateTime date)
        {
            return template
                .Replace("{yyyy}", date.Year.ToString("0000"))
                .Replace("{MM}", date.Month.ToString("00"))
                .Replace("{dd}", date.Day.ToString("00"));
        }
    }
}
