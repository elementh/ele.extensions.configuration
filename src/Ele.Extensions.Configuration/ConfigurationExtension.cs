using System;
using Microsoft.Extensions.Configuration;

namespace Ele.Extensions.Configuration
{
    /// <summary>
    ///     Common set of configuration settings.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        ///     Builds an IConfiguration.
        /// </summary>
        /// <param name="directory">Directory of the project.</param>
        /// <returns>An instance of <see cref="IConfiguration" /></returns>
        public static IConfiguration LoadConfiguration(string directory)
        {
            return new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .AddJsonFile("appsettings.Local.json", true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}