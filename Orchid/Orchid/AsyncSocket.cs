// Project by Bauss
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Orchid
{
	/// <summary>
	/// An asynchronous socket.
	/// </summary>
	public sealed class AsyncSocket
	{
		/// <summary>
		/// Creates a new asynchronous socket.
		/// </summary>
		/// <param name="socket">The associated socket.</param>
		public AsyncSocket(Socket socket)
		{
			Socket = socket;
			Socket.Blocking = false;
			
			ReadResult = new AsyncSocketReadResult(this);
		}
		
		/// <summary>
		/// Gets the associated socket.
		/// </summary>
		public Socket Socket { get; private set; }
		
		/// <summary>
		/// Gets the current read result of the socket.
		/// </summary>
		public AsyncSocketReadResult ReadResult { get; private set; }
		
		/// <summary>
		/// Will handle the read of the socket.
		/// </summary>
		/// <param name="readContext">The read context to use.</param>
		/// <remarks>This call starts an asynchronous task for reading.</remarks>
		public void Read(AsyncSocketReadContext readContext)
		{
			Task.Run(async() => await ReadAsync(readContext));
		}
		
		/// <summary>
		/// Reads data from the socket asynchronously.
		/// </summary>
		/// <param name="readContext">The read context.</param>
		/// <returns>A non-data task.</returns>
		private async Task ReadAsync(AsyncSocketReadContext readContext)
		{
			while (Socket.Connected)
			{
				var result = await readContext.HandleInternal(this);
				
				switch (result.State)
				{
					case AsyncSocketReadContextState.Error:
						{
							readContext.HandleError(this);
							
							goto case AsyncSocketReadContextState.LostConnection;
						}
						
					case AsyncSocketReadContextState.LostConnection:
						{
							try
							{
								Socket.Disconnect(false);
								Socket.Shutdown(SocketShutdown.Both);
							}
							catch { }
							
							readContext.HandleDisconnect(this);
							break;
						}
						
					case AsyncSocketReadContextState.ReadAllData:
						{
							result.State = readContext.HandleRead(result);
							
							if (result.State == AsyncSocketReadContextState.ReadAllData)
							{
								result.Buffer = new byte[0];
							}
							else if (result.State == AsyncSocketReadContextState.Error)
							{
								goto case AsyncSocketReadContextState.Error;
							}
							else if (result.State == AsyncSocketReadContextState.LostConnection)
							{
								goto case AsyncSocketReadContextState.LostConnection;
							}
							
							break;
						}
						
					// implicit: case AsyncSocketReadContextState.AwaitingData:
					default:
						{
							// Do nothing ...
							break;
						}
				}
			}
		}
	}
}
