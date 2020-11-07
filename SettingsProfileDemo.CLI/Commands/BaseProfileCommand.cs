using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using SettingsProfileDemo.BusinessLayer.Interfaces;
using SettingsProfileDemo.BusinessLayer.Models;
using SettingsProfileDemo.CLI.Enums;

namespace SettingsProfileDemo.CLI.Commands
{
    [Command]

    public abstract class BaseProfileCommand : ICommand
    {
        protected readonly IProfileManagerService _profileManagerService;

        protected BaseProfileCommand(IProfileManagerService profileManagerService)
        {
            _profileManagerService = profileManagerService;
           
        }

        [CommandOption("profile", 'p',
            IsRequired = true,
            Description = "profile for which to run the command")]
        public string ProfileName { get; set; }

        public async Task<ProfileModel> GetProfileModelAsync()
        {
            try
            {
                return await _profileManagerService.GetProfileAsync(ProfileName);
            }
            catch (KeyNotFoundException knfe)
            {
                throw new CommandException(knfe.Message, (int) ExitCodesEnum.NoProfileByName);
            }
        }

        public abstract ValueTask ExecuteAsync(IConsole console);
    }
}