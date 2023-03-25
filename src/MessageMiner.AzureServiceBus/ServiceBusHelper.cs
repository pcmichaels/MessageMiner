using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageMiner.AzureServiceBus
{
    internal class ServiceBusHelper
    {
        private static ServiceBusHelper? _instance;
        private static string? _connectionString;

        private ServiceBusHelper(string connectionString)
        {
            ServiceBusClient = new ServiceBusClient(connectionString);
            ServiceBusManagementClient = new ServiceBusAdministrationClient(connectionString);
        }

        public ServiceBusClient ServiceBusClient { get; init; }
        public ServiceBusAdministrationClient ServiceBusManagementClient { get; }

        public static void Initialise(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static ServiceBusHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (string.IsNullOrWhiteSpace(_connectionString))
                    {
                        throw new Exception("Connection string must not be null");
                    }
                    _instance = new ServiceBusHelper(_connectionString);
                }
                return _instance;
            }
        }
        
    }

}
