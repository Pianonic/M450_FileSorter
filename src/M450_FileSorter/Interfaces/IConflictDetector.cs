using M450_FileSorter.Models;

namespace M450_FileSorter.Interfaces
{
    public interface IConflictDetector
    {
        IEnumerable<RuleConflict> DetectConflicts(IEnumerable<IRule> rules);
    }
}
