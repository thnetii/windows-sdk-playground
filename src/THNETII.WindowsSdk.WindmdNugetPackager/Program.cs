using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using THNETII.AzureDevOps.Pipelines.Logging;
using THNETII.NuGet.Logging;

namespace THNETII.WindowsSdk.WindmdNugetPackager
{
    using NuGetLogger = global::NuGet.Common.ILogger;

    public static class Program
    {
        private static ICommandHandler Handler { get; } = CommandHandler.Create(
        (IHost host, CancellationToken cancelToken) =>
        {
            var services = host.Services;
            var loggerFactory = services.GetService<ILoggerFactory>() ??
                Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(Program));

            logger.LogInformation("Hello World");

            
        });

        public static Task<int> Main(string[] args)
        {
            var def = new ProgramDefinition();
            var parser = new CommandLineBuilder(def.RootCommand)
                .UseDefaults()
                .UseHost(hostArgs =>
                {
                    var hostBuilder = Host.CreateDefaultBuilder(hostArgs);

                    hostBuilder.ConfigureAppConfiguration((context, config) =>
                    {
                        var fileProvider = new EmbeddedFileProvider(typeof(Program).Assembly);
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
                    });

                    return hostBuilder;
                }, host =>
                {
                    host.ConfigureLogging(logging =>
                    {
                        bool isCiBuild =
#if DEBUG
                            true
#else
                            THNETII.TypeConverter.BooleanStringConverter
                                .ParseOrDefault(Environment.GetEnvironmentVariable("TF_BUILD"),
                                @default: false) 
#endif
                            ;
                        if (isCiBuild)
                        {
                            logging.AddVsoConsole();
                        }
                    });
                    host.ConfigureServices((context, services) =>
                    {
                        services.AddSingleton(def);
                        services.AddOptions<InvocationLifetimeOptions>()
                            .Configure<IConfiguration>((opts, config) =>
                                config.Bind("Lifetime", opts));
                        services.AddTransient<NuGetLogger, NuGetWrapperLogger>();
                    });
                })
                .Build();
            return parser.InvokeAsync(args ?? Array.Empty<string>());
        }

        internal class ProgramDefinition
        {
            public ProgramDefinition()
            {
                RootCommand = new RootCommand { Handler = Handler };

                SdkVersionOption = new Option<string[]>("--sdk")
                {
                    Name = nameof(SdkVersionOption),
                    Description = $"Windows SDK version (4 components), " +
                        "all: Use all available Windows SDK versions, " +
                        "current: Use Windows SDK registered as current, " +
                        "Multiple versions may be specified",
                    Argument =
                    {
                        Name = "SDK",
                        Description = "4-component version number, all or current"
                    }
                };
                SdkVersionOption.AddAlias("-s");
                RootCommand.AddOption(SdkVersionOption);
            }

            public RootCommand RootCommand { get; }
            public Option<string[]> SdkVersionOption { get; }
        }
    }
}
