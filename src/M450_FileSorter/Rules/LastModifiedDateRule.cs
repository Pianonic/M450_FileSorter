using M450_FileSorter.Models;

namespace M450_FileSorter.Rules
{
    public class LastModifiedDateRule : BaseRule
    {
        private readonly int? _newerThanDays;

        public LastModifiedDateRule(string name, string target, Dictionary<string, string> parameters)
            : base(name, target, parameters)
        {
            if (parameters.TryGetValue("newer_than_days", out var daysStr) && int.TryParse(daysStr, out var days))
            {
                _newerThanDays = days;
            }
        }

        public override bool Matches(FileMetadata fileMetadata)
        {
            if (!MatchesPattern(fileMetadata.FileName))
            {
                return false;
            }

            if (_newerThanDays.HasValue)
            {
                if (fileMetadata.LastWriteTime < DateTime.UtcNow.AddDays(-_newerThanDays.Value))
                {
                    return false;
                }
            }
            return true;
        }

        public override string GetTargetDirectory(FileMetadata fileMetadata)
        {
            return ReplaceDatePlaceholders(TargetTemplate, fileMetadata.LastWriteTime);
        }
    }
}
