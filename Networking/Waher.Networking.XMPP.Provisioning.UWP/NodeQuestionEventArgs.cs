﻿using System;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Abstract base class containing event arguments for node question events.
	/// </summary>
	public abstract class NodeQuestionEventArgs : QuestionEventArgs
	{
		private readonly string[] serviceTokens;
		private readonly string[] deviceTokens;
		private readonly string[] userTokens;
		private readonly string nodeId = string.Empty;
		private readonly string sourceId = string.Empty;
		private readonly string partition = string.Empty;

		/// <summary>
		/// Abstract base class containing event arguments for node question events.
		/// </summary>
		/// <param name="Client">Provisioning Client used.</param>
		/// <param name="e">Message with request.</param>
		public NodeQuestionEventArgs(ProvisioningClient Client, MessageEventArgs e)
			: base(Client, e)
		{
			this.serviceTokens = XML.Attribute(e.Content, "st").Split(space, StringSplitOptions.RemoveEmptyEntries);
			this.deviceTokens = XML.Attribute(e.Content, "dt").Split(space, StringSplitOptions.RemoveEmptyEntries);
			this.userTokens = XML.Attribute(e.Content, "ut").Split(space, StringSplitOptions.RemoveEmptyEntries);

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "nd")
				{
					this.nodeId = XML.Attribute(E, "id");
					this.sourceId = XML.Attribute(E, "src");
					this.partition = XML.Attribute(E, "pt");

					break;
				}
			}
		}

		private static readonly char[] space = new char[] { ' ' };

		/// <summary>
		/// Any service tokens available in the request.
		/// </summary>
		public string[] ServiceTokens => this.serviceTokens;

		/// <summary>
		/// And device tokens available in the request.
		/// </summary>
		public string[] DeviceTokens => this.deviceTokens;

		/// <summary>
		/// And user tokens available in the request.
		/// </summary>
		public string[] UserTokens => this.userTokens;

		/// <summary>
		/// Node ID, if applicable.
		/// </summary>
		public string NodeId => this.nodeId;

		/// <summary>
		/// Source ID, if applicable.
		/// </summary>
		public string SourceId => this.sourceId;

		/// <summary>
		/// Partition, if applicable.
		/// </summary>
		public string Partition => this.partition;

	}
}
