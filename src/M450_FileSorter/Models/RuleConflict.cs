using M450_FileSorter.Interfaces;

namespace M450_FileSorter.Models
{
    public record RuleConflict(IRule RuleA, IRule RuleB, string Reason);
}
