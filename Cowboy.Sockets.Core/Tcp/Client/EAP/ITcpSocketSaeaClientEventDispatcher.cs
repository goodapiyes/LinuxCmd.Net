using System.Threading.Tasks;

namespace Cowboy.Sockets.Core
{
    public interface ITcpSocketSaeaClientEventDispatcher
    {
        Task OnServerConnected(TcpSocketSaeaClient client);
        Task OnServerDataReceived(TcpSocketSaeaClient client, byte[] data, int offset, int count);
        Task OnServerDisconnected(TcpSocketSaeaClient client);
    }
}
