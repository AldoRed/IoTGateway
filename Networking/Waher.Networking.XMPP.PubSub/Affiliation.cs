﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Contains information about a user affiliation.
	/// </summary>
	public class Affiliation
    {
		private readonly string node;
		private readonly string jid;
		private readonly AffiliationStatus status;

		/// <summary>
		/// Contains information about a user affiliation.
		/// </summary>
		/// <param name="Node">Node name</param>
		/// <param name="Jid">JID receiving notifications</param>
		/// <param name="Status">Status of the subscription</param>
		public Affiliation(string Node, string Jid, AffiliationStatus Status)
		{
			this.node = Node;
			this.jid = Jid;
			this.status = Status;
		}

		/// <summary>
		/// Node name.
		/// </summary>
		public string Node => this.node;

		/// <summary>
		/// JID receiving notifications.
		/// </summary>
		public string Jid => this.jid;

		/// <summary>
		/// User affiliation.
		/// </summary>
		public AffiliationStatus Status => this.status;
	}
}
