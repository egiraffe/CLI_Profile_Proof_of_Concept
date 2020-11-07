using Microsoft.Extensions.DependencyInjection;
using SettingsProfileDemo.CLI.Commands;

namespace SettingsProfileDemo.CLI.Extensions
{
    public static class CommandRegistrationExtension
    {
        public static IServiceCollection RegisterCommands(this IServiceCollection sc)
        {
            sc.AddTransient<ViewConfigCommand>();
            sc.AddTransient<ConfigureProfileCommand>();
            sc.AddTransient<RemoveProfileCommand>();
            sc.AddTransient<ListProfilesCommand>();
            
            return sc;
        }
    }
}