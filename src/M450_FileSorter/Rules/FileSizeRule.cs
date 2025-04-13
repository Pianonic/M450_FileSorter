using M450_FileSorter.Models;

namespace M450_FileSorter.Rules
{
    public class FileSizeRule : BaseRule
    {
        private readonly long? _minSizeBytes;
        private readonly long? _maxSizeBytes;

        public FileSizeRule(string name, string target, Dictionary<string, string> parameters)
            : base(name, target, parameters)
        {
            if (parameters.TryGetValue("min_size_mb", out var minMbStr) && double.TryParse(minMbStr, out var minMb))
            {
                _minSizeBytes = (long)(minMb * 1024 * 1024);
            }
            if (parameters.TryGetValue("max_size_mb", out var maxMbStr) && double.TryParse(maxMbStr, out var maxMb))
            {
                _maxSizeBytes = (long)(maxMb * 1024 * 1024);
            }
        }

        public override bool Matches(FileMetadata fileMetadata)
        {
            if (!MatchesPattern(fileMetadata.FileName))
            {
                return false;
            }

            bool minOk = !_minSizeBytes.HasValue || fileMetadata.Size >= _minSizeBytes.Value;
            bool maxOk = !_maxSizeBytes.HasValue || fileMetadata.Size <= _maxSizeBytes.Value;

            return minOk && maxOk;
        }

        public override string GetTargetDirectory(FileMetadata fileMetadata)
        {
            return TargetTemplate;
        }
    }
}
