// Project by Bauss
using System;
using System.Net.Sockets;

namespace Orchid
{
	/// <summary>
	/// An asynchronous read result.
	/// </summary>
	public sealed class AsyncSocketReadResult
	{
		/// <summary>
		/// Creates a new asynchronous read result for a socket.
		/// </summary>
		/// <param name="socket">The socket.</param>
		public AsyncSocketReadResult(AsyncSocket socket)
		{
			Socket = socket;
			Buffer = new byte[0];
			SocketError = SocketError.Success;
		}
		
		/// <summary>
		/// Gets the socket associated with the result.
		/// </summary>
		public AsyncSocket Socket { get; private set; }
		
		/// <summary>
		/// Gets or sets the current buffer.
		/// </summary>
		/// <remarks>This buffer is the buffer you should handle on completed reads.</remarks>
		public byte[] Buffer { get; set; }
		
		/// <summary>
		/// Gets or sets the current state of the read result.
		/// </summary>
		public AsyncSocketReadContextState State { get; set; }
		
		/// <summary>
		/// Gets the current read buffer of the socket.
		/// </summary>
		/// <remarks>
		/// This buffer may not contain all bytes and will be cleared on every receive-round.
		/// Do not rely on this buffer for data, please use "Buffer" instead.
		/// </remarks>
		public byte[] ReadBuffer { get; internal set; }
		
		/// <summary>
		/// Gets all current available bytes in the read buffer.
		/// </summary>
		public int AvailableBytes { get; internal set; }
		
		/// <summary>
		/// Gets the last socket error during receive.
		/// </summary>
		public SocketError SocketError { get; internal set; }
	}
}
