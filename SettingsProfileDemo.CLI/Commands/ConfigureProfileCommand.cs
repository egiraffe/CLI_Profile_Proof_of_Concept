using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using SettingsProfileDemo.BusinessLayer.Enums;
using SettingsProfileDemo.BusinessLayer.Extensions;
using SettingsProfileDemo.BusinessLayer.Interfaces;
using SettingsProfileDemo.CLI.ArgumentConverters;
using SettingsProfileDemo.CLI.Extensions;

namespace SettingsProfileDemo.CLI.Commands
{
    [Command("configprofile", Description = "Create or Update Profile Configuration")]
    public class ConfigureProfileCommand : BaseProfileCommand
    {
        public ConfigureProfileCommand(IProfileManagerService profileManagerService) : base(profileManagerService)
        {
        }
        [CommandOption("clientId", 'c', 
            Converter = typeof(GuidArgumentConverter), 
            Description = "CAP Client Identifier",
            IsRequired = true)]
        public Guid ClientId { get; set; }
        
        [CommandOption("clientSecret", 's', 
            Description = "CAP Client Secret",
            IsRequired = true)]
        public string ClientSecret { get; set; }
        
        public override async ValueTask ExecuteAsync(IConsole console)
        {
            if (!console.GetCancellationToken().IsCancellationRequested)
            {
                try
                {
                    await _profileManagerService.CreateOrUpdateProfile(ProfileName, ClientId, ClientSecret);
                }
                catch (Exception e)
                {
                    throw new CommandException(e.Message, (int) ExitCodesEnum.InvalidParameter);
                }

                await console.Output.WriteLineAsync(JsonSerializer.Serialize(
                    await GetProfileModelAsync(),
                    JsonSerializerExtension.SerializerOptions));
            }
        }
    }
}