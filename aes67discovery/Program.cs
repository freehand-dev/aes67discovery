using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace aes67discovery
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSystemd()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.Sources.Clear();
                    IHostEnvironment env = builderContext.HostingEnvironment;

                    var workingDirectory = env.ContentRootPath;
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        workingDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FreeHand", env.ApplicationName);
                        config.SetBasePath(workingDirectory);
                    }
                    else if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        workingDirectory = System.IO.Path.Combine($"/opt/", env.ApplicationName, "etc", env.ApplicationName);
                        config.SetBasePath(workingDirectory);
                    }

                    //
                    Console.WriteLine($"$Env:EnvironmentName={ env.EnvironmentName }");
                    Console.WriteLine($"$Env:ApplicationName={ env.ApplicationName }");
                    Console.WriteLine($"$Env:ContentRootPath={ env.ContentRootPath }");
                    Console.WriteLine($"WorkingDirectory={ workingDirectory }");

                    config.AddIniFile($"{ env.ApplicationName }.conf", optional: true, reloadOnChange: true);
                    config.AddCommandLine(args);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<SAPSettings>(hostContext.Configuration.GetSection("SAP"));
                    services.AddHostedService<Worker>();
                })
                .ConfigureLogging((builderContext, logging) =>
                {
                    logging.AddConsole();
                });
    }
}
