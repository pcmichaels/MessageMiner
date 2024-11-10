using MessageMiner.Common;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace MessageMiner.Detect;

public enum TraceState
{
    Injected,
    Detected,
    Unknown
}

public class Trace
{
    public int Index { get; set; }
    public string? CorrelationId { get; set; }
    public string? Topic { get; set; }
    public DateTime InjectedDateStamp { get; set; }    
    public TraceState TraceState { get; set; }
    public List<Detected> Detected { get; set; } = new List<Detected>();
}

public class Detected
{
    public Detected(string subscription, DateTime detectedTime)
    {
        Subscription = subscription;
        DetectedDateStamp = detectedTime;
    }
    public string Subscription { get; set; }
    public DateTime DetectedDateStamp { get; set; }

}

public class Track
{
    private readonly ISend _send;
    private readonly IReceive _receive;
    private readonly IManagement _management;
    private readonly ILogger<Track> _logger;
    private string _message;

    private List<Trace> _traces = new List<Trace>();

    public Track(ISend send, IReceive receive, 
        IManagement management, IConfiguration configuration,
        ILogger<Track> logger)
    {
        _send = send;
        _receive = receive;
        _management = management;
        _logger = logger;
    }

    public async void Configure(string message)
    {            
        _message = message;
    }

    public async Task<List<Trace>> Detect()
    {
        _logger.LogInformation($"Detecting {_traces.Count} traces");

        int foundCount = 0;
        foreach (var trace in _traces)
        {
            ArgumentNullException.ThrowIfNull(trace.Topic);
            ArgumentNullException.ThrowIfNull(trace.CorrelationId);

            var subscriptions = await _management.GetAllSubscriptions(trace.Topic);

            foreach (var subscription in subscriptions)
            {
                _logger.LogInformation($"Analysing in {trace.Topic} / {subscription}");

                if (await _receive.ReceiveCorrelationId(
                    trace.CorrelationId, trace.Topic, subscription))
                {
                    trace.TraceState = TraceState.Detected;                    
                    trace.Detected.Add(new Detected(subscription, DateTime.Now));
                    foundCount++;

                    _logger.LogInformation($"Detected {trace.CorrelationId} in {trace.Topic} / {subscription}");
                }
            }
        }
        return _traces;
    }

    public Task DetectDuplicates()
    {
        throw new NotImplementedException();
    }

    public async Task<List<string>> GetDeadLetterQueues()
    {
        var deadLetterQueues = await _management.GetAllDeadLetterQueues();
        return deadLetterQueues;
    }

    public async Task<int> Inject()
    {
        _logger.LogInformation("Finding All Topics...");
        var topics = await _management.GetAllTopics();
        int idx = 0;

        _logger.LogInformation($"Processing {topics.Count} topics...");

        foreach (var topic in topics)
        {
            var correlationId = Guid.NewGuid().ToString();
            await _send.SendWithCorrelationId(correlationId,
                topic, _message);
            _traces.Add(new Trace()
            {
                Index = idx++,
                CorrelationId = correlationId,
                InjectedDateStamp = DateTime.Now,
                Topic = topic,
                TraceState = TraceState.Injected
            });

            _logger.LogInformation($"Injected {correlationId}");
        }

        return idx;
    }
}