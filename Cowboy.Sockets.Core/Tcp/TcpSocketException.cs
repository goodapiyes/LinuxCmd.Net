﻿using System;

namespace Cowboy.Sockets.Core
{
    [Serializable]
    public class TcpSocketException : Exception
    {
        public TcpSocketException(string message)
            : base(message)
        {
        }

        public TcpSocketException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
