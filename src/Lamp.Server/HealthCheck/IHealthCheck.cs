using System.Threading.Tasks;

namespace Lamp.Server.HealthCheck
{
    public interface IHealthCheck
    {
        Task RunAsync();
    }
}
