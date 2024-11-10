using Azure.Messaging.ServiceBus.Administration;
using MessageMiner.Common;

// https://pmichaels.net/2022/08/21/listing-all-topics-and-subscriptions-in-an-azure-service-bus-namespace/

namespace MessageMiner.AzureServiceBus
{
    public class Management : IManagement
    {
        public Management(IConfiguration configuration)
        {
            ServiceBusHelper.Initialise(((Configuration)configuration).ConnectionString);
        }

        public async Task<List<string>> GetAllDeadLetterQueues()
        {
            List<string> deadLetterQueues = new List<string>();

            var managementClient = ServiceBusHelper.Instance
                .ServiceBusManagementClient;

            var pageableQueues = managementClient
                .GetQueuesAsync();

            var queues = pageableQueues.AsPages();
            await foreach (var queue in queues)
            { 
                var dlqs = queue.Values
                    .Where(q => q.Status == EntityStatus.ReceiveDisabled 
                    && q.Name.EndsWith("/$DeadLetterQueue"));

                foreach (var dlq in dlqs)
                {
                    deadLetterQueues.Add(dlq.Name);
                }                
            }

            return deadLetterQueues;
        }

        public async Task<List<string>> GetAllDeadLetterTopics()
        {
            List<string> deadLetterQueues = new List<string>();

            var managementClient = ServiceBusHelper.Instance
                .ServiceBusManagementClient;

            var pageableQueues = managementClient
                .GetTopicsAsync();

            var topics = pageableQueues.AsPages();
            await foreach (var queue in topics)
            {
                var dlqs = queue.Values
                    .Where(q => q.Status == EntityStatus.ReceiveDisabled
                    && q.Name.EndsWith("/$DeadLetterQueue"));

                foreach (var dlq in dlqs)
                {
                    deadLetterQueues.Add(dlq.Name);
                }
            }

            return deadLetterQueues;
        }

        public async Task<IList<string>> GetAllSubscriptions(string topic)
        {
            var subs = ServiceBusHelper.Instance.ServiceBusManagementClient.GetSubscriptionsAsync(topic);
            List<string> subscriptionList = new List<string>();

            await foreach (var sub in subs)
            {
                if (sub.ForwardTo != null)
                {
                    continue;
                }

                subscriptionList.Add(sub.SubscriptionName);
            }

            return subscriptionList;

        }

        public async Task<IList<string>> GetAllTopics()
        {                        
            var topics = ServiceBusHelper.Instance.ServiceBusManagementClient.GetTopicsAsync();
            List<string> topicList = new List<string>();

            await foreach (var topic in topics)
            {
                topicList.Add(topic.Name);
            }

            return topicList;
        }
    }
}
