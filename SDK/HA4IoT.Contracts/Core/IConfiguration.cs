using System.Threading.Tasks;

namespace HA4IoT.Contracts.Core
{
    public interface IConfiguration
    {
        Task ApplyAsync();
    }
}
