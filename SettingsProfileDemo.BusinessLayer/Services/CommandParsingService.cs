using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SettingsProfileDemo.BusinessLayer.Interfaces;

namespace SettingsProfileDemo.BusinessLayer.Services
{
    public class CommandParsingService : ICommandParsingService
    {
        // looks for up to to dashes signifying a command then arguments split on word boundry or quoted word boundry (single or double)
        private static readonly Regex _commandParseRegex= new Regex(
            @"(?<Command>\w*(?>\s?))(?<Parameter>(?<ParameterIdentifier>[-]{1,2}[\w]+)\s?(?<ParameterArgument>\S+|"".*?\""|'.*?\')*)?");

        public string[] ParseCommandString(string command)
        {
            var arguments = new List<string>();
            
            if (string.IsNullOrWhiteSpace(command))
            {
                return arguments.ToArray();
            }

            var matches = _commandParseRegex.Matches(command).ToArray();

            foreach (var match in matches)
            {
                if (!string.IsNullOrWhiteSpace(match.Groups["Command"]?.Value)) arguments.Add(match.Groups["Command"].Value.Trim());
                if (!string.IsNullOrWhiteSpace(match.Groups["ParameterIdentifier"]?.Value)) arguments.Add(match.Groups["ParameterIdentifier"].Value);
                if (!string.IsNullOrWhiteSpace(match.Groups["ParameterArgument"]?.Value)) arguments.Add(match.Groups["ParameterArgument"].Value.Trim('\'').Trim('"'));
            }

            return arguments.ToArray();
        }
    }
}