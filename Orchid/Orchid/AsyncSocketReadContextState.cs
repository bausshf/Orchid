// Project by Bauss
using System;

namespace Orchid
{
	/// <summary>
	/// Enumeration of read-context states for an asynchronous socket.
	/// </summary>
	public enum AsyncSocketReadContextState
	{
		/// <summary>
		/// An empty state. This is the default state.
		/// </summary>
		None,
		
		/// <summary>
		/// An error has occurred during the last read.
		/// </summary>
		Error,
		
		/// <summary>
		/// The socket has lost its connection or has been terminated.
		/// Note: This may also be invoked by "Error"
		/// </summary>
		LostConnection,
		
		/// <summary>
		/// The socket is currently awaiting data.
		/// </summary>
		AwaitingData,
		
		/// <summary>
		/// The socket has read all data.
		/// </summary>
		ReadAllData
	}
}
