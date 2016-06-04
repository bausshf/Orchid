// Project by Bauss
using System;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Orchid
{
	/// <summary>
	/// An asynchronous read context.
	/// </summary>
	public abstract class AsyncSocketReadContext
	{
		/// <summary>
		/// Creates a new asynchronous read context.
		/// </summary>
		/// <param name="roundBufferSize">The size of each read-round buffer. Typically this would be 1024.</param>
		protected AsyncSocketReadContext(int roundBufferSize)
		{
			RoundBufferSize = roundBufferSize;
		}
		
		/// <summary>
		/// Gets the size of the read-round buffer.
		/// </summary>
		public int RoundBufferSize { get; private set; }
		
		/// <summary>
		/// Handles the read internally asynchronously.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <returns>The socket's read result.</returns>
		internal async Task<AsyncSocketReadResult> HandleInternal(AsyncSocket socket)
		{
			if (socket.ReadResult.ReadBuffer == null && socket.ReadResult.State == AsyncSocketReadContextState.None || socket.ReadResult.State == AsyncSocketReadContextState.ReadAllData)
			{
				socket.ReadResult.ReadBuffer = new byte[RoundBufferSize];
				
				socket.ReadResult.State = AsyncSocketReadContextState.AwaitingData;
			}
			
			var result = await Task.Run(() => Read(socket));
			
			return result;
		}
		
		/// <summary>
		/// Reads data from the socket.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <returns>The socket's read result.</returns>
		private AsyncSocketReadResult Read(AsyncSocket socket)
		{
			if (socket.Socket.Poll(1, SelectMode.SelectRead))
			{
				SocketError socketError;
				socket.ReadResult.AvailableBytes = socket.Socket.Receive(socket.ReadResult.ReadBuffer, 0, socket.ReadResult.ReadBuffer.Length, SocketFlags.None, out socketError);
				
				if (socketError != SocketError.Success)
				{
					socket.ReadResult.SocketError = socketError;
					socket.ReadResult.State = AsyncSocketReadContextState.Error;
					
					return socket.ReadResult;
				}
				
				if (socket.ReadResult.AvailableBytes == 0)
				{
					socket.ReadResult.State = AsyncSocketReadContextState.LostConnection;
				}
				else
				{
					if (socket.ReadResult.ReadBuffer.Length > 0)
					{
						var temp = new byte[socket.ReadResult.Buffer.Length + socket.ReadResult.AvailableBytes];
						
						System.Buffer.BlockCopy(socket.ReadResult.Buffer, 0, temp, 0, socket.ReadResult.Buffer.Length);
						System.Buffer.BlockCopy(socket.ReadResult.ReadBuffer, 0, temp, socket.ReadResult.Buffer.Length, socket.ReadResult.AvailableBytes);
						
						socket.ReadResult.Buffer = temp;
					}
					
					socket.ReadResult.ReadBuffer = new byte[this.RoundBufferSize];
					socket.ReadResult.State = AsyncSocketReadContextState.AwaitingData;
				}
				
				if (socket.ReadResult.State == AsyncSocketReadContextState.AwaitingData || socket.ReadResult.State == AsyncSocketReadContextState.None)
				{
					socket.ReadResult.State = GetState(socket.ReadResult);
				}
			}
			else if (socket.Socket.Poll(1, SelectMode.SelectError))
			{
				socket.ReadResult.State = AsyncSocketReadContextState.Error;
			}
			
			return socket.ReadResult;
		}
		
		/// <summary>
		/// Gets the state of the read result.
		/// </summary>
		/// <param name="readResult">The socket's read result.</param>
		/// <returns>The state of the read result.</returns>
		protected abstract AsyncSocketReadContextState GetState(AsyncSocketReadResult readResult);
		
		/// <summary>
		/// Handles a complete read.
		/// </summary>
		/// <param name="readResult">The socket's read result.</param>
		/// <returns>The state of the read result.</returns>
		/// <remarks>The buffer of the read result may contain bytes from the next packet. Thus make sure you handle packet-splitting.</remarks>
		public abstract AsyncSocketReadContextState HandleRead(AsyncSocketReadResult readResult);
		
		/// <summary>
		/// Handles socket errors.
		/// </summary>
		/// <param name="client">The client socket.</param>
		/// <remarks>Do NOT disconnect the socket here. It will be automatically disconnected on errors.</remarks>
		public abstract void HandleError(AsyncSocket client);
		
		/// <summary>
		/// Handles the socket's disconnect.
		/// </summary>
		/// <param name="client">The client socket disconnected.</param>
		public abstract void HandleDisconnect(AsyncSocket client);
	}
}
