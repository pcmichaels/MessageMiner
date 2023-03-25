﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageMiner.Common
{
    public interface IReceive
    {
        Task<bool> ReceiveCorrelationId(string correlationId, string topicName, string subscription);
    }
}
