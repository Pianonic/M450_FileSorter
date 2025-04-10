using M450_FileSorter.Interfaces;
using M450_FileSorter.Models;
using M450_FileSorter.Rules;

namespace M450_FileSorter.Factories
{
    public class RuleFactory : IRuleFactory
    {
        public IEnumerable<IRule> CreateRules(IEnumerable<RuleConfig> ruleConfigs)
        {
            var rules = new List<IRule>();
            foreach (var config in ruleConfigs)
            {
                try
                {
                    IRule? rule = config.Type.ToLowerInvariant() switch
                    {
                        "creation_date" => new CreationDateRule(config.Name, config.Target, config.Params),
                        "last_modified_date" => new LastModifiedDateRule(config.Name, config.Target, config.Params),
                        "file_size" => new FileSizeRule(config.Name, config.Target, config.Params),
                        "regex" => new RegexRule(config.Name, config.Target, config.Params),
                        _ => null
                    };

                    if (rule != null)
                    {
                        rules.Add(rule);
                        Console.WriteLine($"[Info] Loaded rule: '{rule.Name}' ({rule.GetType().Name})");
                    }
                    else
                    {
                        Console.WriteLine($"[Warning] Unknown rule type '{config.Type}' for rule '{config.Name}'. Skipping.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Failed to create rule '{config.Name}': {ex.Message}. Skipping.");
                }
            }
            return rules;
        }
    }
}
