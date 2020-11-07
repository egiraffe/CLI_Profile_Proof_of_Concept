using System;
using System.Collections.Generic;
using System.Linq;

namespace SettingsProfileDemo.CLI.AutoCompletion
{
    public class AutoCompletionHandler : IAutoCompleteHandler
    {
        public string[] GetSuggestions(string text, int index)
        {
            if (AutoCompletionMatrix.ContainsKey(text))
            {
                return AutoCompletionMatrix[text].ToArray();
            }

            var suspectedKey = AutoCompletionMatrix.Keys.OrderBy(ob => ob).FirstOrDefault(f => f.StartsWith(text));

            return string.IsNullOrWhiteSpace(suspectedKey)
                ? new string[0]
                : AutoCompletionMatrix[suspectedKey].ToArray();
        }
        
        private readonly Dictionary<string, List<string>> AutoCompletionMatrix = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"config ", new List<string>{"configprofile"}},
            {"delete ", new List<string>{"deleteprofile"}},
            {"view ", new List<string>{"viewprofile", "viewprofilelist"}},
            {"help", new List<string>{"help"}},
            {"history", new List<string>{"history"}},
            {"exit", new List<string>{"exit"}},
            {"quit", new List<string>{"exit"}},
            {"clear", new List<string>{"clear"}},
        };

        public char[] Separators { get; set; } = new[] {' '};
    }
}