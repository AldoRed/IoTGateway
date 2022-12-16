﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Networking.XMPP.Interoperability
{
	/// <summary>
	/// Delegate for interoperability interfaces event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task InteroperabilityServerInterfacesEventHandler(object Sender, InteroperabilityServerEventArgs e);

	/// <summary>
	/// Event arguments for interoperability interface requests.
	/// </summary>
	public class InteroperabilityServerEventArgs : EventArgs
	{
		private ThingReference thingRef = null;
		private readonly List<string> interfaces = new List<string>();
		private readonly string nodeId;
		private readonly string sourceId;
		private readonly string partition;
		private readonly string serviceToken;
		private readonly string deviceToken;
		private readonly string userToken;

		/// <summary>
		/// Event arguments for interoperability interface requests.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="SourceId">Source ID</param>
		/// <param name="Partition">Partition</param>
		/// <param name="ServiceToken">Service Token</param>
		/// <param name="DeviceToken">Device Token</param>
		/// <param name="UserToken">User Token</param>
		public InteroperabilityServerEventArgs(string NodeId, string SourceId, string Partition, string ServiceToken, string DeviceToken, string UserToken)
		{
			this.nodeId = NodeId;
			this.sourceId = SourceId;
			this.partition = Partition;
			this.serviceToken = ServiceToken;
			this.deviceToken = DeviceToken;
			this.userToken = UserToken;
		}

		/// <summary>
		/// Node ID
		/// </summary>
		public string NodeId => this.nodeId;

		/// <summary>
		/// Source ID
		/// </summary>
		public string SourceId => this.sourceId;

		/// <summary>
		/// Partition
		/// </summary>
		public string Partition => this.partition;

		/// <summary>
		/// Service Token
		/// </summary>
		public string ServiceToken => this.serviceToken;

		/// <summary>
		/// Device Token
		/// </summary>
		public string DeviceToken => this.deviceToken;

		/// <summary>
		/// User Token
		/// </summary>
		public string UserToken => this.userToken;

		/// <summary>
		/// Thing reference.
		/// </summary>
		public ThingReference ThingReference
		{
			get
			{
				if (this.thingRef is null)
					this.thingRef = new ThingReference(this.nodeId, this.sourceId, this.partition);

				return this.thingRef;
			}
		}

		/// <summary>
		/// Adds interoperability interfaces to the response.
		/// </summary>
		/// <param name="Interfaces">Interfaces</param>
		public void Add(params string[] Interfaces)
		{
			this.interfaces.AddRange(Interfaces);
		}

		/// <summary>
		/// Reported Interoperability Interfaces.
		/// </summary>
		public string[] Interfaces
		{
			get { return this.interfaces.ToArray(); }
		}

	}
}
