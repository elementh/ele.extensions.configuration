using System;
using System.Reflection;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;
using Serilog.Sinks.AwsCloudWatch.LogStreamNameProvider;

namespace Ele.Extensions.Configuration
{
    /// <summary>
    /// Common set of configuration settings.
    /// </summary>
    public static class ConfigurationExtension
    {
        /// <summary>
        /// Builds an IConfiguration.
        /// </summary>
        /// <param name="directory">Directory of the project.</param>
        /// <returns></returns>
        public static IConfiguration LoadConfiguration(string directory)
        {
            return new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .AddJsonFile($"appsettings.Local.json", true)
                .AddEnvironmentVariables()
                .Build();
        }

        /// <summary>
        /// Creates a logger using Serilog.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="service"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
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
            
            var client = new AmazonCloudWatchLogsClient(new BasicAWSCredentials(configuration["AWS_ACCESS_KEY"], configuration["AWS_SECRET_KEY"]), RegionEndpoint.EUWest1);

            var loggerConf = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.AmazonCloudWatch(cloudWatchSink, client);

            if (environment == "Development")
            {
                loggerConf.WriteTo.Console();
                loggerConf.MinimumLevel.Debug();
            }
            else
            {
                loggerConf.MinimumLevel.Warning();
            }

            return loggerConf.CreateLogger();
        }
    }
}