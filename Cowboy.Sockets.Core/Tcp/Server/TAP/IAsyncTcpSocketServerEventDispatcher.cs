﻿using System.Threading.Tasks;

namespace Cowboy.Sockets.Core
{
    public interface IAsyncTcpSocketServerEventDispatcher
    {
        Task OnSessionStarted(AsyncTcpSocketSession session);
        Task OnSessionDataReceived(AsyncTcpSocketSession session, byte[] data, int offset, int count);
        Task OnSessionClosed(AsyncTcpSocketSession session);
    }
}
