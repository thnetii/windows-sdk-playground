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

namespace THNETII.WindowsSdk.WindmdNugetPackager
{
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

            var cmd = services.GetRequiredService<ParseResult>();
            var def = services.GetRequiredService<ProgramDefinition>();
            string? value = cmd.FindResultFor(def.ValueArgument) is { } valueResult
                ? valueResult.GetValueOrDefault<string>() : null;
            if (!string.IsNullOrWhiteSpace(value))
                logger.LogInformation($"Argument passed: {{{nameof(value)}}}", value);
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
                    //host.ConfigureLogging(logging =>
                    //{
                    //    bool isCiBuild = BooleanStringConverter.ParseOrDefault(
                    //        Environment.GetEnvironmentVariable("TF_BUILD"),
                    //        @default: false);
                    //    if (isCiBuild)
                    //    {
                    //        logging.AddVsoConsole();
                    //    }
                    //});
                    host.ConfigureServices((context, services) =>
                    {
                        services.AddSingleton(def);
                        services.AddOptions<InvocationLifetimeOptions>()
                            .Configure<IConfiguration>((opts, config) =>
                                config.Bind("Lifetime", opts));
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

                ValueArgument = new Argument<string>("VALUE")
                {
                    Description = "A value to be logged",
                    Arity = ArgumentArity.ZeroOrOne
                };
                RootCommand.AddArgument(ValueArgument);
            }

            public RootCommand RootCommand { get; }
            public Argument<string> ValueArgument { get; }
        }
    }
}
