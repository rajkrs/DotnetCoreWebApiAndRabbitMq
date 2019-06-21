using RabbitMQ.Client;
using System.Threading.Tasks;

namespace MessageQ
{
    public interface IMqPublisher
    {
        Task PublishAsync(Event @message, int? retryCount = null);

    }
}
