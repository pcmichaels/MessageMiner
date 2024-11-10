using MessageMiner.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageMiner.AzureServiceBus
{
    public class Configuration : IConfiguration
    {
        public string? ConnectionString { get; set; }
    }
}
