using System.Text.Json;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using SettingsProfileDemo.BusinessLayer.Extensions;
using SettingsProfileDemo.BusinessLayer.Interfaces;
using SettingsProfileDemo.CLI.Extensions;

namespace SettingsProfileDemo.CLI.Commands
{
    [Command("viewprofile", Description = "View a profile's configuration")]
    public class ViewConfigCommand : BaseProfileCommand
    {
        public ViewConfigCommand(
            IProfileManagerService profileManagerService) : 
            base(profileManagerService)
        {
        }

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var cancellation = console.GetCancellationToken();
            var serialized = JsonSerializer
                    .Serialize(await GetProfileModelAsync(), JsonSerializerExtension.SerializerOptions);
            
            if (!cancellation.IsCancellationRequested)
            {
                await console.Output.WriteLineAsync(serialized);
            }
        }
    }
}