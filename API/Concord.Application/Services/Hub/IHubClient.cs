using Concord.Application.DTO.Notifications;

namespace Concord.Application.Services.Hub
{
    public interface IHubClient
    {
        Task CustomerCreateOrder(NewCreatedOrderMsg msg);
    }
}
