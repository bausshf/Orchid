// Example by Bauss
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Orchid;

namespace ServerSample
{
	/// <summary>
	/// Example on a tcp server.
	/// </summary>
	static class Program
	{
		/// <summary>
		/// The server socket.
		/// </summary>
		private static AsyncTcpServerSocket _server;
		
		/// <summary>
		/// The read context for clients.
		/// </summary>
		/// <remarks>Each client can have their own read contexts if chosen.</remarks>
		private static TcpReadContext _readContext;
		
		/// <summary>
		/// A custom tcp read context.
		/// </summary>
		private class TcpReadContext : AsyncSocketReadContext
		{
			/// <summary>
			/// Creates a new custom tcp read context with a round-read buffer size of 1024.
			/// </summary>
			public TcpReadContext()
				: base(1024)
			{
				
			}
			
			// For summary read AsyncSocketReadContext.GetState()
			protected override AsyncSocketReadContextState GetState(AsyncSocketReadResult readResult)
			{
				// Get size of packet
				ushort packetSize = 0;
				
				unsafe
				{
					fixed (byte* ptr = readResult.Buffer)
					{
						packetSize = (*(ushort*)(ptr));
					}
				}
				
				// If the size is negative or the size is above 1024 then the packet is invalid.
				if (packetSize <= 0 || packetSize > 1024)
				{
					return AsyncSocketReadContextState.Error;
				}
				
				// Checks whether we have received the whole packet or not
				return readResult.Buffer.Length >= packetSize ?
					AsyncSocketReadContextState.ReadAllData : AsyncSocketReadContextState.AwaitingData;
			}
			
			// For summary read AsyncSocketReadContext.HandleRead()
			// Note: This is just one way that reads can be handled
			//       Packets can be handled however chosen.
			public override AsyncSocketReadContextState HandleRead(AsyncSocketReadResult readResult)
			{
				// Get size of packet
				ushort packetSize = 0;
				
				unsafe
				{
					fixed (byte* ptr = readResult.Buffer)
					{
						packetSize = (*(ushort*)(ptr));
					}
				}
				
				// The packet size is above 0
				if (packetSize > 0)
				{
					// The packet example is a message, so we take one byte (the packet size) away from the message length
					var messageBuffer = new byte[packetSize - 2];
					System.Buffer.BlockCopy(readResult.Buffer, 2, messageBuffer, 0, messageBuffer.Length);
					var message = System.Text.Encoding.ASCII.GetString(messageBuffer);
					
					Console.WriteLine("Received message: {0}", message);
				}
				
				// Check if we have received either whole or partial bytes of next packet
				if (readResult.Buffer.Length > packetSize)
				{
					// Copy bytes from next packet into buffer
					var temp = new byte[readResult.Buffer.Length - packetSize];
					System.Buffer.BlockCopy(readResult.Buffer, packetSize, temp, 0, temp.Length);
					
					readResult.Buffer = temp;
					
					// Get the state of the next packet
					var nextPacketState = GetState(readResult);
					
					// Did we receive the whole next packet?
					if (nextPacketState == AsyncSocketReadContextState.ReadAllData)
					{
						// The next packet was received whole with current packet
						readResult.State = AsyncSocketReadContextState.ReadAllData;
						
						// Handle the next packet
						return HandleRead(readResult);
					}
					
					return nextPacketState;
				}
				
				return AsyncSocketReadContextState.ReadAllData;
			}
			
			// For summary read AsyncSocketReadContext.HandleError()
			public override void HandleError(AsyncSocket client)
			{
				Console.WriteLine("Socket error: {0}", client.ReadResult.SocketError);
			}
			
			// For summary read AsyncSocketReadContext.HandleDisconnect()
			public override void HandleDisconnect(AsyncSocket client)
			{
				Console.WriteLine("Disconnected: {0}", (client.Socket.RemoteEndPoint as IPEndPoint).Address.ToString());
			}
		}
		
		public static void Main(string[] args)
		{
			// Creates the asynchronous tcp server socket
			_server = new AsyncTcpServerSocket(new AsyncSocketConfiguration
			                                   {
			                                   	// Sets the end point of the socket
			                                   	EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9968),
			                                   	
			                                   	// Sets the backlog
			                                   	Backlog = 500
			                                   });
			// Creates our custom tcp read context
			_readContext = new TcpReadContext();
			
			// Opens the server socket
			_server.Open();
			
			// Creates an asynchronous task to handle accepts
			Task.Run(async () => await HandleAccept());
			
			// Client example ...
			ClientExample();
		}
		
		/// <summary>
		/// Handles accepts of clients.
		/// </summary>
		/// <returns>A no-data task.</returns>
		private static async Task HandleAccept()
		{
			while (_server.Running)
			{
				// Will wait asynchronously for a socket to be accepted.
				var client = await _server.AcceptAsync();
				
				Console.WriteLine("Accepted a connection from {0}", (client.Socket.RemoteEndPoint as IPEndPoint).Address.ToString());
				
				// Begins to handle the read of the connected socket.
				// We pass our custom read context to handle reads for this socket
				client.Read(_readContext);
			}
		}
		
		/// <summary>
		/// An example of a simple client to use for our server.
		/// </summary>
		private static void ClientExample()
		{
			var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			
			client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9968));
			
			while (client.Connected)
			{
				var message = Console.ReadLine();
				
				if (message == "dc")
				{
					client.Disconnect(false);
				}
				
				var messageBytes = System.Text.Encoding.ASCII.GetBytes(message);
				var buffer = new byte[2 + messageBytes.Length];
				
				System.Buffer.BlockCopy(messageBytes, 0, buffer, 2, messageBytes.Length);
				
				unsafe
				{
					fixed (byte* ptr = buffer)
					{
						(*(ushort*)(ptr)) = (ushort)(messageBytes.Length + 2);
					}
				}
				
				client.Send(buffer);
				
				System.Threading.Thread.Sleep(100);
			}
			
			// Exit ...
			Console.ReadLine();
		}
	}
}