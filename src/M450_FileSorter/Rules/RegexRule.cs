using M450_FileSorter.Models;
using System.Text.RegularExpressions;

namespace M450_FileSorter.Rules
{
    public class RegexRule : BaseRule
    {
        private readonly Regex _mainRegex;

        public RegexRule(string name, string target, Dictionary<string, string> parameters)
            : base(name, target, parameters)
        {
            if (!parameters.TryGetValue("pattern", out var mainPattern) || string.IsNullOrWhiteSpace(mainPattern))
                throw new ArgumentException($"Regex rule '{name}' requires a 'pattern' parameter.");
            try
            {
                _mainRegex = new Regex(mainPattern, RegexOptions.Compiled);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Invalid main regex pattern for rule '{Name}': {mainPattern}. Error: {ex.Message}", ex);
            }
        }

        public override bool Matches(FileMetadata fileMetadata)
        {
            if (!base.MatchesPattern(fileMetadata.FileName)) // Prüft optionales Base-Pattern
                return false;

            return _mainRegex.IsMatch(fileMetadata.FileName); // Prüft Haupt-Pattern
        }

        public override string GetTargetDirectory(FileMetadata fileMetadata)
        {
            var match = _mainRegex.Match(fileMetadata.FileName);
            if (!match.Success)
                return TargetTemplate;

            string resolvedTarget = TargetTemplate;
            for (int i = 1; i < match.Groups.Count; i++)
                resolvedTarget = resolvedTarget.Replace($"{{${i}}}", match.Groups[i].Value);

            return resolvedTarget;
        }
    }
}
