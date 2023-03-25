namespace MessageMiner.Common
{
    public interface IManagement
    {
        Task<IList<string>> GetAllTopics();
        Task<IList<string>> GetAllSubscriptions(string topic);
    }
}