using M450_FileSorter.Interfaces;
using M450_FileSorter.Models;

namespace M450_FileSorter.ConflictDetection
{
    public class SimpleConflictDetector : IConflictDetector
    {
        public IEnumerable<RuleConflict> DetectConflicts(IEnumerable<IRule> rules)
        {
            var conflicts = new List<RuleConflict>();
            var ruleList = rules.ToList();

            for (int i = 0; i < ruleList.Count; i++)
            {
                for (int j = i + 1; j < ruleList.Count; j++)
                {
                    if (ruleList[i].TargetTemplate.Equals(ruleList[j].TargetTemplate, StringComparison.OrdinalIgnoreCase))
                    {
                        conflicts.Add(new RuleConflict(ruleList[i], ruleList[j], $"Both rules point to the same target template: '{ruleList[i].TargetTemplate}'"));
                    }
                }
            }

            return conflicts;
        }
    }
}
