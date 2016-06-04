// Project by Bauss
using System;
using System.Net;
using System.Net.Sockets;

namespace Orchid
{
	/// <summary>
	/// An asynchronous socket configuration wrapper.
	/// </summary>
	public sealed class AsyncSocketConfiguration
	{
		/// <summary>
		/// Gets or sets the end point of the socket.
		/// </summary>
		public IPEndPoint EndPoint { get; set; }
		
		/// <summary>
		/// Gets or sets the backlog of the socket.
		/// </summary>
		public int Backlog { get; set; }
	}
}
