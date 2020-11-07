using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using Microsoft.Extensions.DependencyInjection;
using SettingsProfileDemo.BusinessLayer.Enums;
using SettingsProfileDemo.BusinessLayer.Extensions;
using SettingsProfileDemo.BusinessLayer.Interfaces;
using SettingsProfileDemo.CLI.Extensions;

namespace SettingsProfileDemo.CLI
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // create a di container
            var services = new ServiceCollection();
            
            // register services
            services.RegisterServices();
            
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
                        commandText = ReadLine.Read(string.Empty, string.Empty);
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

                    var parsedArguments = commandParsingService.ParseCommandString(commandText);
                    
                    await app.RunAsync(parsedArguments);
                }
                
                Environment.Exit((int) ExitCodesEnum.Normal);
            }

            // handle passed in arguments
            return await app.RunAsync(args);
        }
    }
}