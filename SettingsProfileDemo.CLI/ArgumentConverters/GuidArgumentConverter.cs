using System;
using CliFx;

namespace SettingsProfileDemo.CLI.ArgumentConverters
{
    public class GuidArgumentConverter : IArgumentValueConverter
    {
        public object ConvertFrom(string value)
        {
            return string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var guid)
            ? Guid.Empty
            : guid;
        }
    }
}