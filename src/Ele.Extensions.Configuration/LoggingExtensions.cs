using System;
using System.Reflection;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;
using Serilog.Sinks.AwsCloudWatch.LogStreamNameProvider;

namespace Ele.Extensions.Configuration
{
    /// <summary>
    ///     Custom logging loader.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        ///     Creates a logger using Serilog.
        /// </summary>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>An instance of <see cref="ILogger" /></returns>
        public static ILogger LoadLogger(IConfiguration configuration)
        {
            var environment = $"{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}";

            var cloudWatchSink = new CloudWatchSinkOptions
            {
                LogGroupName = $"{configuration["LOG_GROUP_NAME"]}#{environment}",
                LogStreamNameProvider = new ConfigurableLogStreamNameProvider($"{Assembly.GetCallingAssembly().GetName().Name}", false, false),
                CreateLogGroup = false,
                TextFormatter = new CompactJsonFormatter(),
                MinimumLogEventLevel = environment == "Development" ? LogEventLevel.Debug : LogEventLevel.Information
            };

            var client = new AmazonCloudWatchLogsClient(new BasicAWSCredentials(configuration["AWS_ACCESS_KEY"], configuration["AWS_SECRET_KEY"]),
                RegionEndpoint.EUWest1);

            var loggerConf = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithDestructurers(new[] {new DbUpdateExceptionDestructurer()}))
                .WriteTo.AmazonCloudWatch(cloudWatchSink, client);

            if (environment == "Development")
                loggerConf.MinimumLevel.Debug();
            else
                loggerConf.MinimumLevel.Information();

            if (environment == "Development")
                loggerConf.WriteTo.Console();
            else if (!string.IsNullOrWhiteSpace(configuration["ENABLE_CONSOLE_LOGS"])) loggerConf.WriteTo.Console();

            return loggerConf.CreateLogger();
        }
    }
}