using Azure.Messaging.ServiceBus.Administration;
using MessageMiner.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// https://pmichaels.net/2022/08/21/listing-all-topics-and-subscriptions-in-an-azure-service-bus-namespace/

namespace MessageMiner.AzureServiceBus
{
    public class Management : IManagement
    {
        public Management(IConfiguration configuration)
        {
            ServiceBusHelper.Initialise(((Configuration)configuration).ConnectionString);

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
