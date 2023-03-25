using MessageMiner.Detect;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageMiner.TestHarness
{
    internal class ConsoleHostedService : IHostedService
    {
        private readonly ILogger<ConsoleHostedService> _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly Track _track;

        public ConsoleHostedService(ILogger<ConsoleHostedService> logger,
               IHostApplicationLifetime appLifetime,
               Track track)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _track = track;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _track.Configure("pcm - test message");

            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select action?")
                        .PageSize(20)
                        .AddChoices(new[] {
                            "1: Inject and Detect Message",
                            "2: Analyse the Dead Letter Queue",
                            "3: Analyse Over Time",
                            "0: Exit"})
                    );

                string[] action = choice.Split(':');

                switch (action[0])
                {
                    case "0":
                        _appLifetime.StopApplication();
                        return;

                    case "1":
                        // Trace Message
                        int count = await _track.Inject();
                        await Task.Delay(5000);
                        var traces = await _track.Detect();

                        var grid = new Grid();
                        grid.AddColumns(5);
                        grid.AddRow(new string[] { "Time Injected", "Topic", "Correlation Id", "Time Detected", "Subscription" });
                        foreach (var trace in traces.OrderBy(a => a.Index))
                        {
                            grid.AddRow(new string[]
                            {
                                trace.InjectedDateStamp.ToString(),                                
                                trace.Topic ?? "NULL",
                                trace.CorrelationId ?? "NULL", 
                                "", ""
                            });
                            foreach (var detected in trace.Detected)
                            {
                                grid.AddRow(new string[]
                                {
                                    "", "", "", 
                                    detected.DetectedDateStamp.ToString(),
                                    detected.Subscription
                                });
                            }
                        }
                        AnsiConsole.Write(grid);
                        
                        break;

                    case "2":
                        // Dead Letter Analyser
                        var deadLetterQueues = _track.GetDeadLetterQueues();
                        await _track.DetectDuplicates();
                        break;

                    case "3":
                        // Time analysis - sets up a subscription to every topic
                        // and monitors over a period for throughput
                        break;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            
        }
    }
}