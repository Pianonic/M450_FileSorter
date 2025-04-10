using M450_FileSorter.Models;

namespace M450_FileSorter.Rules
{
    public class CreationDateRule : BaseRule
    {
        public CreationDateRule(string name, string target, Dictionary<string, string> parameters)
            : base(name, target, parameters) { }

        public override bool Matches(FileMetadata fileMetadata)
        {
            if (!MatchesPattern(fileMetadata.FileName))
            {
                return false;
            }
            return true;
        }

        public override string GetTargetDirectory(FileMetadata fileMetadata)
        {
            return ReplaceDatePlaceholders(TargetTemplate, fileMetadata.CreationTime);
        }
    }
}
