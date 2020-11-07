using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using SettingsProfileDemo.BusinessLayer.Enums;
using SettingsProfileDemo.BusinessLayer.Interfaces;
using SettingsProfileDemo.BusinessLayer.Models;
using SettingsProfileDemo.BusinessLayer.Services;

namespace SettingsProfileDemo.BusinessLayer.Extensions
{
    public static class ServiceRegistrationExtension
    {
        public static IServiceCollection RegisterServices(this IServiceCollection sc)
        {
            sc.AddSingleton<ICommandParsingService, CommandParsingService>();
            var profileManagerService = new ProfileManagerService(HandleNoProfiles);
            sc.AddSingleton<IProfileManagerService, ProfileManagerService>(x=> profileManagerService);
            return sc;
        }
        
        static void HandleNoProfiles(object obj, EventArgs eventArgs)
        {
            Console.WriteLine("Please add a profile to continue using this app");

            if (obj is IProfileManagerService profileManagerService)
            {
                while (!TryAddProfile(profileManagerService, "default"))
                {
                    Console.WriteLine("You must have a profile to continue...");
                }

                return;
            }

            Environment.Exit((int) ExitCodesEnum.NoConfiguredProfiles);
        }
        
        static bool TryAddProfile(IProfileManagerService profileManagerService, string defaultProfileName = "")
        {
            Console.WriteLine("You may type 'cancel' at any time to cancel adding a profile");
            
            var profileName = string.Empty;
            while (string.IsNullOrWhiteSpace(profileName))
            {
                Console.Write($"Profile Name (Hit <Enter> for '{defaultProfileName}'): " );
                profileName = Console.ReadLine() ?? string.Empty;
                
                if (string.IsNullOrEmpty(profileName))
                {
                    profileName = defaultProfileName;
                }
                
                if (profileName.Equals("cancel", StringComparison.InvariantCultureIgnoreCase)) return false;
            }

            var clientId = Guid.Empty;
            while (clientId == Guid.Empty)
            {
                Console.Write("CAP Client Identifier: ");
                var clientIdString = Console.ReadLine() ?? string.Empty;
                if (clientIdString.Equals("cancel", StringComparison.InvariantCultureIgnoreCase)) return false;
                clientId = Guid.TryParse(clientIdString, out var clientGuid) ? clientGuid : Guid.Empty;
            }

            var clientSecret = string.Empty;
            while (string.IsNullOrWhiteSpace(clientSecret))
            {
                Console.Write("CAP Client Secret: " );
                clientSecret = Console.ReadLine() ?? string.Empty;
                if (clientSecret.Equals("cancel", StringComparison.InvariantCultureIgnoreCase)) return false;
            }
            
            var profileModel = new ProfileModel(profileName, clientId, clientSecret);

            try
            {
                profileManagerService.CreateOrUpdateProfile(profileModel).Wait();
            }
            catch (ValidationException ve)
            {
                Console.WriteLine(ve.Message);
                return false;
            }

            return true;
        }
    }
}