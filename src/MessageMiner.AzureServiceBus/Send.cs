using Azure.Messaging.ServiceBus;
using MessageMiner.Common;

namespace MessageMiner.AzureServiceBus
{
    public class Send : ISend
    {
        public Send(IConfiguration configuration)
        {
            ServiceBusHelper.Initialise(((Configuration)configuration).ConnectionString);

        }

        public async Task SendWithCorrelationId(
            string correlationId, string topicName, string messageBody)
        {
            var sender = ServiceBusHelper.Instance
                .ServiceBusClient.CreateSender(topicName);
            var message = new ServiceBusMessage(messageBody);
            message.CorrelationId = correlationId;

            await sender.SendMessageAsync(message);
            await sender.CloseAsync();

        }
    }
}
