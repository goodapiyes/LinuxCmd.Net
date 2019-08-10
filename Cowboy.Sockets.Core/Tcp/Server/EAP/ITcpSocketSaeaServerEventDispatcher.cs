using System.Threading.Tasks;

namespace Cowboy.Sockets.Core
{
    public interface ITcpSocketSaeaServerEventDispatcher
    {
        Task OnSessionStarted(TcpSocketSaeaSession session);
        Task OnSessionDataReceived(TcpSocketSaeaSession session, byte[] data, int offset, int count);
        Task OnSessionClosed(TcpSocketSaeaSession session);
    }
}
