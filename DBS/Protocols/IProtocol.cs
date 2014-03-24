using System.Threading.Tasks;

namespace DBS.Protocols
{
    /// <summary>
    /// Initiator peers
    /// </summary>
    interface IProtocol
    {
        Task Run();
    }
}
