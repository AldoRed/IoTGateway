﻿using System;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event arguments for Update callbacks.
	/// </summary>
	public class UpdateEventArgs : IqResultEventArgs
	{
		private readonly bool disowned;

		internal UpdateEventArgs(IqResultEventArgs e, object State, bool Disowned)
			: base(e)
		{
			this.State = State;
			this.disowned = Disowned;
		}

		/// <summary>
		/// If the thing has been disowned by the owner. In that case, meta-data was not updated. An unclaimed device can re-register
		/// with the registry, if it wants to update its meta-data.
		/// </summary>
		public bool Disowned => this.disowned;
	}
}
