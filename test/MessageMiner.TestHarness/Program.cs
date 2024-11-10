using MessageMiner.Common;
using MessageMiner.Detect;
using MessageMiner.TestHarness;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MessageMiner.AzureServiceBus;
using Microsoft.Extensions.Logging;

Microsoft.Extensions.Configuration.IConfiguration configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", true, true)
              .Build();

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<ConsoleHostedService>();
        services.AddSingleton<Track>();
        services.AddSingleton<ISend, Send>();
        services.AddSingleton<IReceive, Receive>();
        services.AddSingleton<IManagement, Management>();
        services.AddSingleton<MessageMiner.Common.IConfiguration>(new Configuration()
        {
            ConnectionString = configuration.GetValue<string>("ConnectionString")            
        });
        services.AddLogging(a => a.AddConsole());
    })
    .RunConsoleAsync();
