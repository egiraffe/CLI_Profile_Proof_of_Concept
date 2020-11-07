using System;
using Microsoft.Extensions.DependencyInjection;
using SettingsProfileDemo.CLI.AutoCompletion;

namespace SettingsProfileDemo.CLI.Extensions
{
    public static class ReadLineExtensions
    {
        public static IServiceCollection RegisterReadLine(this IServiceCollection sc)
        {
            ReadLine.AutoCompletionHandler = new AutoCompletionHandler();
            ReadLine.HistoryEnabled = true;
            return sc;
        }
    }
}