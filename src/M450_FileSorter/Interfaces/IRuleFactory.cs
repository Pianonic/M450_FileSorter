using M450_FileSorter.Models;

namespace M450_FileSorter.Interfaces
{
    public interface IRuleFactory
    {
        IEnumerable<IRule> CreateRules(IEnumerable<RuleConfig> ruleConfigs);
    }
}
