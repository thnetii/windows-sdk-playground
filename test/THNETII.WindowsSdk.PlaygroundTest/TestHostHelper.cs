using System;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace THNETII.WindowsSdk.PlaygroundTest
{
    public static class TestHostHelper
    {
        public static IHostBuilder CreateHostBuilder(string[]? args) =>
            Host.CreateDefaultBuilder(args ?? Array.Empty<string>())
                .UseEnvironment("Testing")
                .ConfigureLogging(logging =>
                {
                    var consoleService = logging.Services.FirstOrDefault(desc =>
                        desc.ServiceType == typeof(ILoggerProvider) && desc.ImplementationType == typeof(ConsoleLoggerProvider)
                        );
                    if (consoleService is ServiceDescriptor)
                        logging.Services.Remove(consoleService);
                })
                .ConfigureAppConfiguration(config => config.AddUserSecrets(typeof(TestHostHelper).Assembly, optional: true))
                .ConfigureAppConfiguration((context, config) =>
                {
                    var fileProvider = new EmbeddedFileProvider(typeof(TestHostHelper).Assembly);
                    var hostingEnvironment = context.HostingEnvironment;

                    var sources = config.Sources;
                    int originalSourcesCount = sources.Count;

                    config.AddJsonFile(fileProvider,
                        $"appsettings.json",
                        optional: true, reloadOnChange: true);
                    config.AddJsonFile(fileProvider,
                        $"appsettings.{hostingEnvironment.EnvironmentName}.json",
                        optional: true, reloadOnChange: true);

                    const int insert_idx = 1;
                    for (int i_dst = insert_idx, i_src = originalSourcesCount;
                        i_src < sources.Count; i_dst++, i_src++)
                    {
                        var configSource = sources[i_src];
                        sources.RemoveAt(i_src);
                        sources.Insert(i_dst, configSource);
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddOptions<ConsoleLifetimeOptions>()
                        .Configure<IConfiguration>((opts, config) =>
                            config.Bind("Lifetime", opts));
                });
    }
}
