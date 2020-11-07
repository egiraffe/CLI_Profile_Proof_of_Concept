using System;
using System.Text.Json;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using SettingsProfileDemo.BusinessLayer.Interfaces;
using SettingsProfileDemo.CLI.Enums;

namespace SettingsProfileDemo.CLI.Commands
{
    [Command ("viewprofilelist", Description = "View a list of configured profiles")]
    public class ListProfilesCommand : ICommand
    {
        private readonly IProfileManagerService _profileManagerService;

        public ListProfilesCommand(IProfileManagerService profileManagerService)
        {
            _profileManagerService = profileManagerService;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            try
            {
                var profiles = await _profileManagerService.GetProfilesAsync();
                await console.Output.WriteLineAsync(JsonSerializer.Serialize(profiles.Keys));
            }
            catch (Exception e)
            {
                throw new CommandException($"there was an error listing profiles {e.Message}", (int) ExitCodesEnum.GeneralError);
            }
        }
    }
}