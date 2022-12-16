﻿using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.InBandBytestreams
{
	/// <summary>
	/// Delegate for data reception callback methods.
	/// </summary>
	/// <param name="Sender">Caller.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task DataReceivedEventHandler(object Sender, DataReceivedEventArgs e);

	/// <summary>
	/// Event arguments containing received binary data.
	/// </summary>
	public class DataReceivedEventArgs : EventArgs
	{
		private readonly byte[] data;
		private readonly object state;

		/// <summary>
		/// Event arguments containing received binary data.
		/// </summary>
		/// <param name="Data">Data received.</param>
		/// <param name="State">State object.</param>
		public DataReceivedEventArgs(byte[] Data, object State)
		{
			this.data = Data;
			this.state = State;
		}

		/// <summary>
		/// Binary data received.
		/// </summary>
		public byte[] Data => this.data;

		/// <summary>
		/// State object.
		/// </summary>
		public object State => this.state;
	}
}
