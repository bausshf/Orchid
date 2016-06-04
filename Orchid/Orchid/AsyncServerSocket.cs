// Project by Bauss
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Orchid
{
	/// <summary>
	/// An asynchronous server implementation.
	/// </summary>
	public abstract class AsyncServerSocket
	{
		/// <summary>
		/// The association socket.
		/// </summary>
		private readonly Socket _serverSocket;
		
		/// <summary>
		/// The socket configuration.
		/// </summary>
		private readonly AsyncSocketConfiguration _config;
		
		/// <summary>
		/// Creates a new asynchronous server socket.
		/// </summary>
		/// <param name="config">The configuration of the socket.</param>
		/// <param name="protocolType">The protocol type of the socket.</param>
		protected AsyncServerSocket(AsyncSocketConfiguration config, ProtocolType protocolType)
		{
			_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
			
			_config = config;
		}
		
		/// <summary>
		///  Gets a boolean determining whether the server is running or not.
		/// </summary>
		public bool Running { get; private set; }
		
		/// <summary>
		/// Opens the server socket.
		/// </summary>
		public void Open()
		{
			_serverSocket.Bind(_config.EndPoint);
			_serverSocket.Listen(_config.Backlog);
			
			Running = true;
		}
		
		/// <summary>
		/// Closes the server socket.
		/// </summary>
		public void Close()
		{
			_serverSocket.Close();
			
			Running = false;
		}
		
		/// <summary>
		/// Accepts socket connections asynchronously.
		/// </summary>
		/// <returns>The accepted socket.</returns>
		public async Task<AsyncSocket> AcceptAsync()
		{
			var socket = await Task.Run<AsyncSocket>(() => new AsyncSocket(_serverSocket.Accept()));
			
			return socket;
		}
	}
}
