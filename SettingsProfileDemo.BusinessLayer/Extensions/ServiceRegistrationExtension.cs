using System;
using Microsoft.Extensions.DependencyInjection;
using SettingsProfileDemo.BusinessLayer.Interfaces;
using SettingsProfileDemo.BusinessLayer.Services;

namespace SettingsProfileDemo.BusinessLayer.Extensions
{
    public static class ServiceRegistrationExtension
    {
        public static IServiceCollection RegisterServices(this IServiceCollection sc, EventHandler handleNoProfiles)
        {
            sc.AddSingleton<ICommandParsingService, CommandParsingService>();
            var profileManagerService = new ProfileManagerService(handleNoProfiles);
            sc.AddSingleton<IProfileManagerService, ProfileManagerService>(x=> profileManagerService);
            return sc;
        }
    }
}