// Project by Bauss
using System;
using System.Net.Sockets;

namespace Orchid
{
	/// <summary>
	/// An asynchronous tcp server socket.
	/// </summary>
	public sealed class AsyncTcpServerSocket : AsyncServerSocket
	{
		/// <summary>
		/// Creates a new asynchronous tcp server socket.
		/// </summary>
		/// <param name="config">The configuration of the server socket.</param>
		public AsyncTcpServerSocket(AsyncSocketConfiguration config)
			: base(config, ProtocolType.Tcp)
		{
		}
	}
}
