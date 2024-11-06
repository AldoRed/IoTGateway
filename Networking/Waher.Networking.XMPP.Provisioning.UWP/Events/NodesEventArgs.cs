﻿using Waher.Things;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Provisioning.Events
{
	/// <summary>
	/// Base class for CanRead and CanControl callback event arguments.
	/// </summary>
	public class NodesEventArgs : JidEventArgs
	{
		private readonly IThingReference[] nodes;

		internal NodesEventArgs(IqResultEventArgs e, object State, string JID, IThingReference[] Nodes)
			: base(e, State, JID)
		{
			this.nodes = Nodes;
		}

		/// <summary>
		/// Nodes allowed to process. If null, no node restrictions exist.
		/// </summary>
		public IThingReference[] Nodes => this.nodes;
	}
}
