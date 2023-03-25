namespace MessageMiner.Common
{
    public interface ISend
    {
        Task SendWithCorrelationId(string correlationId,
            string topicName, string message);
    }
}