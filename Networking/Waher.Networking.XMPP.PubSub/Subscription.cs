﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Contains information about a subscription.
	/// </summary>
	public class Subscription
    {
		private readonly string node;
		private readonly string jid;
		private readonly string subscriptionId;
		private readonly NodeSubscriptionStatus status;

		/// <summary>
		/// Contains information about a subscription.
		/// </summary>
		/// <param name="Node">Node name</param>
		/// <param name="Jid">JID receiving notifications</param>
		/// <param name="Status">Status of the subscription</param>
		/// <param name="SubscriptionId">Subscription ID</param>
		public Subscription(string Node, string Jid, NodeSubscriptionStatus Status, string SubscriptionId)
		{
			this.node = Node;
			this.jid = Jid;
			this.status = Status;
			this.subscriptionId = SubscriptionId;
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
		/// State of the subscription.
		/// </summary>
		public NodeSubscriptionStatus Status => this.status;

		/// <summary>
		/// Subscription ID
		/// </summary>
		public string SubscriptionId => this.subscriptionId;
	}
}
