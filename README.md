# ele.extensions.configuration
Configuration and logging extension.

[I](https://lucasmarino.me) use this nuget or a copy of this code on almost every project I work at. It provides easy serilog configuration and AWS Cloudwatch logging.

See [Ele.Extensions.Configuration at nuget.org](https://www.nuget.org/packages/Ele.Extensions.Configuration/)

## Usage

```csharp
namespace Example.Project
{
    public class Program
    {
        private static IConfiguration Configuration { get; } = ConfigurationExtension.LoadConfiguration(Directory.GetCurrentDirectory()); // Here

        public static void Main(string[] args)
        {
            var assembly = Assembly.GetCallingAssembly().GetName().Name;

            Log.Logger = ConfigurationExtension.LoadLogger(Configuration); // Here

            try
            {
                var host = CreateHostBuilder(args).Build();

                Log.Information($"Starting {assembly}");

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"{assembly} terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseLamar()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddControllers();
                    });
                    webBuilder.UseSerilog(); // Here
                });
    }
}
```

## Contribution

I'm open to contributions, but only if they make sense ;)

## License
Ele.Extensions.Configuration

Copyright (C) 2021  Lucas Maximiliano Marino <https://lucasmarino.me>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.