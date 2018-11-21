using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Shared.DTO.Analytics;

namespace Adaptive.ReactiveTrader.Server.Analytics
{
    public interface IAnalyticsPublisher
    {
        Task Publish(PositionUpdatesDto positionUpdatesDto);
    }
}