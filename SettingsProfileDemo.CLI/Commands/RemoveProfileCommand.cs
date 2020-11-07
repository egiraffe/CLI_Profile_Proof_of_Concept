using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using SettingsProfileDemo.BusinessLayer.Interfaces;

namespace SettingsProfileDemo.CLI.Commands
{
    [Command ("deleteprofile", Description = "Delete the profile from the configuration file")]
    public class RemoveProfileCommand : BaseProfileCommand
    {
        public RemoveProfileCommand(IProfileManagerService profileManagerService) : base(profileManagerService)
        {
        }

        [CommandOption("skipConfirmation", 's',
            IsRequired = false,
            Description = "Skip Confirmation Acknowledgement")]
        public bool SkipConfirmation { get; set; } = false;
        
        public override async ValueTask ExecuteAsync(IConsole console)
        {
            try
            {
                var confirmed = SkipConfirmation;
                
                if (!confirmed)
                {
                    var response = 0;
                    while (response == 0)
                    {
                        await console.Output.WriteAsync("Are you sure you want to remove this profile? (y/n): ");
                        response = console.Input.Read();
                        if (response != 'y' && response != 'Y' && response != 'n' && response != 'N') response = 0;
                        await console.Output.WriteLineAsync();
                    }

                    confirmed = response == 'y' || response == 'Y';
                }

                if (confirmed)
                {
                    await _profileManagerService.DeleteProfileAsync(ProfileName);
                    await console.Output.WriteLineAsync($"{ProfileName} has been successfully deleted");
                }
            }
            catch (Exception e)
            {
                throw new CommandException($"there was an error deleting the requested profile {e}");
            }
        }
    }
}