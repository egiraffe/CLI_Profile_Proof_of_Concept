using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using Microsoft.Extensions.DependencyInjection;
using SettingsProfileDemo.BusinessLayer.Extensions;
using SettingsProfileDemo.BusinessLayer.Interfaces;
using SettingsProfileDemo.BusinessLayer.Models;
using SettingsProfileDemo.CLI.Enums;
using SettingsProfileDemo.CLI.Extensions;

namespace SettingsProfileDemo.CLI
{
    internal class Program
    {
        private static string[] initialArgs;
        public static async Task<int> Main(string[] args)
        {
            initialArgs = args;
            
            // create a di container
            var services = new ServiceCollection();
            
            // register services
            services.RegisterServices(HandleNoProfiles);
            
            // register commands
            services.RegisterCommands();
            
            // register readline helper
            services.RegisterReadLine();
            
            // build service provider
            var sp = services.BuildServiceProvider();

            var app = new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(sp.GetService)
                .Build();
            
            // handle interactive cli
            if (args == null || args.Length == 0)
            {
                var commandParsingService = sp.GetRequiredService<ICommandParsingService>();
                var commandText = string.Empty;

                while (!commandText.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.Write("> ");
                    commandText = string.Empty;
                    while (string.IsNullOrWhiteSpace(commandText))
                    {
                        commandText = ReadLine.Read(string.Empty, string.Empty).Trim();
                    }

                    if (commandText.Equals("clear", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.Clear();
                        continue;
                    }
                    
                    if (commandText.Equals("history", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var history in ReadLine.GetHistory().ToArray().Reverse().Take(10))
                        {
                            Console.WriteLine(history);
                        }

                        continue;
                    }

                    if (commandText.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    var parsedArguments = commandParsingService.ParseCommandString(commandText);
                    
                    await app.RunAsync(parsedArguments);
                }
                
                Environment.Exit((int) ExitCodesEnum.Normal);
            }

            // handle passed in arguments
            return await app.RunAsync(args);
        }
        
        #region helpers
        
        static void HandleNoProfiles(object obj, EventArgs eventArgs)
        {
            if (initialArgs != null && 
                initialArgs.Length == 1 &&
                initialArgs[0].Contains("help", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            
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
        
        #endregion
    }
}