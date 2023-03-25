using Azure.Messaging.ServiceBus;
using MessageMiner.Common;
using Microsoft.Extensions.Logging;

namespace MessageMiner.AzureServiceBus
{
    public class Receive : IReceive
    {
        private readonly ILogger<IReceive> _logger;

        public Receive(IConfiguration configuration, ILogger<IReceive> logger)
        {
            ServiceBusHelper.Initialise(((Configuration)configuration).ConnectionString);
            _logger = logger;
        }

        public async Task<bool> ReceiveCorrelationId(string correlationId, string topicName, string subscription)
        {
            var subscriptionInformation = (await ServiceBusHelper.Instance
                .ServiceBusManagementClient.GetSubscriptionRuntimePropertiesAsync(
                    topicName, subscription)).Value;
            _logger.LogInformation($"Subscription: {subscription}, active messages: {subscriptionInformation.ActiveMessageCount}");
            int processedCount = 0;
            int quarter = (int)(subscriptionInformation.ActiveMessageCount / 4);

            var receiver = ServiceBusHelper.Instance
                .ServiceBusClient.CreateReceiver(topicName, subscription,
                new ServiceBusReceiverOptions()
                {
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                });

            var messages = receiver.ReceiveMessagesAsync();   
            
            await foreach (var message in messages)
            {
                if (quarter > 0 && ((subscriptionInformation.ActiveMessageCount - ++processedCount) % quarter == 0))
                {
                    _logger.LogInformation($"{processedCount} messages completed...");
                }
                if (message.CorrelationId != correlationId)
                {
                    await receiver.AbandonMessageAsync(message);
                    continue;
                }

                return true;
            }

            return false;
        }

    }
}