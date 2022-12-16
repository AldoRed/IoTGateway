﻿using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Type of presence received.
	/// </summary>
	public enum PresenceType
	{
		/// <summary>
		/// Contact is available.
		/// </summary>
		Available,

		/// <summary>
		/// An error has occurred regarding processing of a previously sent presence stanza; if the presence stanza is of type
		/// "error", it MUST include an <error/> child element (refer to [XMPP-CORE]).
		/// </summary>
		Error,

		/// <summary>
		/// A request for an entity's current presence; SHOULD be generated only by a server on behalf of a user.
		/// </summary>
		Probe,

		/// <summary>
		/// The sender wishes to subscribe to the recipient's presence.
		/// </summary>
		Subscribe,

		/// <summary>
		/// The sender has allowed the recipient to receive their presence.
		/// </summary>
		Subscribed,

		/// <summary>
		/// The sender is no longer available for communication.
		/// </summary>
		Unavailable,

		/// <summary>
		/// The sender is unsubscribing from the receiver's presence.
		/// </summary>
		Unsubscribe,

		/// <summary>
		/// The subscription request has been denied or a previously granted subscription has been canceled.
		/// </summary>
		Unsubscribed
	}

	/// <summary>
	/// Resource availability.
	/// </summary>
	public enum Availability
	{
		/// <summary>
		/// The entity or resource is online.
		/// </summary>
		Online,

		/// <summary>
		/// The entity or resource is offline.
		/// </summary>
		Offline,

		/// <summary>
		/// The entity or resource is temporarily away.
		/// </summary>
		Away,

		/// <summary>
		/// The entity or resource is actively interested in chatting.
		/// </summary>
		Chat,

		/// <summary>
		/// The entity or resource is busy.
		/// </summary>
		DoNotDisturb,

		/// <summary>
		/// The entity or resource is away for an extended period.
		/// </summary>
		ExtendedAway
	}

	/// <summary>
	/// Event arguments for presence events.
	/// </summary>
	public class PresenceEventArgs : EventArgs
	{
		private readonly KeyValuePair<string, string>[] statuses;
		private readonly XmlElement presence;
		private XmlElement content;
		private readonly XmlElement errorElement = null;
		private readonly ErrorType errorType = ErrorType.None;
		private readonly XmppException stanzaError = null;
		private readonly string errorText = string.Empty;
		private readonly XmppClient client;
		private readonly XmppComponent component;
		private readonly PresenceType type;
		private readonly Availability availability;
		private readonly DateTime received;
		private readonly string from;
		private readonly string fromBareJid;
		private readonly string to;
		private readonly string id;
		private readonly string status;
		private readonly string entityCapabilityVersion = null;
		private readonly string entityCapabilityNode = null;
		private readonly string entityCapabilityHashFunction = null;
		private readonly int errorCode;
		private readonly sbyte priority;
		private readonly bool hasEntityCapabilities = false;
		private bool ok;
		private bool updateLastPresence = false;
		private bool testing = false;
		private object state = null;

		internal PresenceEventArgs(XmppClient Client, XmlElement Presence)
			: this(Client, null, Presence)
		{
		}

		internal PresenceEventArgs(XmppComponent Component, XmlElement Presence)
			: this(null, Component, Presence)
		{
		}

		/// <summary>
		/// Event arguments for presence events.
		/// </summary>
		/// <param name="e">Copies attributes from this event arguments class.</param>
		protected PresenceEventArgs(PresenceEventArgs e)
		{
			this.statuses = e.statuses;
			this.presence = e.presence;
			this.content = e.content;
			this.errorElement = e.errorElement;
			this.errorType = e.errorType;
			this.stanzaError = e.stanzaError;
			this.errorText = e.errorText;
			this.client = e.client;
			this.component = e.component;
			this.type = e.type;
			this.availability = e.availability;
			this.from = e.from;
			this.fromBareJid = e.fromBareJid;
			this.to = e.to;
			this.id = e.id;
			this.status = e.status;
			this.errorCode = e.errorCode;
			this.priority = e.priority;
			this.ok = e.ok;
			this.received = e.received;
			this.state = e.state;
		}

		private PresenceEventArgs(XmppClient Client, XmppComponent Component, XmlElement Presence)
		{
			XmlElement E;
			int i;

			this.presence = Presence;
			this.client = Client;
			this.component = Component;
			this.from = XML.Attribute(Presence, "from");
			this.to = XML.Attribute(Presence, "to");
			this.id = XML.Attribute(Presence, "id");
			this.ok = true;
			this.errorCode = 0;
			this.availability = Availability.Online;
			this.received = DateTime.Now;

			i = this.from.IndexOf('/');
			if (i < 0)
				this.fromBareJid = this.from;
			else
				this.fromBareJid = this.from.Substring(0, i);

			switch (XML.Attribute(Presence, "type").ToLower())
			{
				case "error":
					this.type = PresenceType.Error;
					break;

				case "probe":
					this.type = PresenceType.Probe;
					break;

				case "subscribe":
					this.type = PresenceType.Subscribe;
					break;

				case "subscribed":
					this.type = PresenceType.Subscribed;
					break;

				case "unavailable":
					this.type = PresenceType.Unavailable;
					this.availability = Availability.Offline;
					break;

				case "unsubscribe":
					this.type = PresenceType.Unsubscribe;
					break;

				case "unsubscribed":
					this.type = PresenceType.Unsubscribed;
					break;

				default:
					this.type = PresenceType.Available;
					break;
			}

			SortedDictionary<string, string> Statuses = new SortedDictionary<string, string>();

			foreach (XmlNode N in Presence.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null)
					continue;

				if (E.NamespaceURI == Presence.NamespaceURI)
				{
					switch (E.LocalName)
					{
						case "show":
							switch (E.InnerText.ToLower())
							{
								case "away":
									this.availability = Availability.Away;
									break;

								case "chat":
									this.availability = Availability.Chat;
									break;

								case "dnd":
									this.availability = Availability.DoNotDisturb;
									break;

								case "xa":
									this.availability = Availability.ExtendedAway;
									break;

								default:
									this.availability = Availability.Online;
									break;
							}
							break;

						case "status":
							if (string.IsNullOrEmpty(this.status))
								this.status = N.InnerText;

							string Language = XML.Attribute(E, "xml:lang");
							Statuses[Language] = N.InnerText;
							break;

						case "priority":
							if (!sbyte.TryParse(N.InnerText, out this.priority))
								this.priority = 0;
							break;

						case "error":
							this.errorElement = E;
							this.errorCode = XML.Attribute(E, "code", 0);
							this.ok = false;

							switch (XML.Attribute(E, "type"))
							{
								case "auth":
									this.errorType = ErrorType.Auth;
									break;

								case "cancel":
									this.errorType = ErrorType.Cancel;
									break;

								case "continue":
									this.errorType = ErrorType.Continue;
									break;

								case "modify":
									this.errorType = ErrorType.Modify;
									break;

								case "wait":
									this.errorType = ErrorType.Wait;
									break;

								default:
									this.errorType = ErrorType.Undefined;
									break;
							}

							this.stanzaError = XmppClient.GetExceptionObject(E);
							this.errorText = this.stanzaError.Message;
							break;
					}
				}
				else if (E.NamespaceURI == XmppClient.NamespaceEntityCapabilities && E.LocalName == "c")
				{
					this.entityCapabilityVersion = XML.Attribute(E, "ver");
					this.entityCapabilityNode = XML.Attribute(E, "node");
					this.entityCapabilityHashFunction = XML.Attribute(E, "hash");
					this.hasEntityCapabilities = true;
				}
				else if (this.content is null)
					this.content = E;
			}

			this.statuses = new KeyValuePair<string, string>[Statuses.Count];
			Statuses.CopyTo(this.statuses, 0);
		}

		/// <summary>
		/// Type of presence received.
		/// </summary>
		public PresenceType Type => this.type;

		/// <summary>
		/// Resource availability.
		/// </summary>
		public Availability Availability => this.availability;

		/// <summary>
		/// From where the presence was received.
		/// </summary>
		public string From => this.from;

		/// <summary>
		/// Bare JID of resource sending the presence.
		/// </summary>
		public string FromBareJID => this.fromBareJid;

		/// <summary>
		/// To whom the presence was sent.
		/// </summary>
		public string To => this.to;

		/// <summary>
		/// ID attribute of presence stanza.
		/// </summary>
		public string Id => this.id;

		/// <summary>
		/// Human readable status.
		/// </summary>
		public string Status => this.status;

		/// <summary>
		/// Presence element.
		/// </summary>
		public XmlElement Presence => this.presence;

		/// <summary>
		/// XMPP Client. Is null if event raised by a component.
		/// </summary>
		public XmppClient Client => this.client;

		/// <summary>
		/// XMPP Component. Is null if event raised by a client.
		/// </summary>
		public XmppComponent Component => this.component;

		/// <summary>
		/// If the connection is being tested.
		/// </summary>
		internal bool Testing
		{
			get => this.testing;
			set => this.testing = value;
		}

		/// <summary>
		/// If the <see cref="RosterItem.LastPresence"/> property should be updated with this presence, for the corresponding contact.
		/// </summary>
		public bool UpdateLastPresence
		{
			get => this.updateLastPresence;
			set => this.updateLastPresence = value;
		}

		/// <summary>
		/// State object, if event a callback from a directed presence request.
		/// </summary>
		public object State
		{
			get => this.state;
			set => this.state = value;
		}

		/// <summary>
		/// If contact is online.
		/// </summary>
		public bool IsOnline
		{
			get { return this.ok && this.availability != Availability.Offline; }
		}

		/// <summary>
		/// Content of the presence stanza. For stanzas that are processed by registered presence handlers, this value points to the 
		/// element inside the presence stanza, that the handler is registered to handle. For other types of presence stanzas, it 
		/// represents the first custom element in the message. If no such elements are found, this value is null.
		/// </summary>
		public XmlElement Content
		{
			get => this.content;
			internal set => this.content = value;
		}

		/// <summary>
		/// If the response is an OK result response (true), or an error response (false).
		/// </summary>
		public bool Ok
		{
			get => this.ok;
			set => this.ok = value;
		}

		/// <summary>
		/// Error Code
		/// </summary>
		public int ErrorCode => this.errorCode;

		/// <summary>
		/// Error Type
		/// </summary>
		public ErrorType ErrorType => this.errorType;

		/// <summary>
		/// Error element.
		/// </summary>
		public XmlElement ErrorElement => this.errorElement;

		/// <summary>
		/// Any error specific text.
		/// </summary>
		public string ErrorText => this.errorText;

		/// <summary>
		/// Any stanza error returned.
		/// </summary>
		public XmppException StanzaError => this.stanzaError;

		/// <summary>
		/// Available set of (language,status) pairs.
		/// </summary>
		public KeyValuePair<string, string>[] Statuses => this.statuses;

		/// <summary>
		/// Priority of presence stanza.
		/// </summary>
		public sbyte Priority => this.priority;

		/// <summary>
		/// Timestamp of when the stanza was received.
		/// </summary>
		public DateTime Received => this.received;

		/// <summary>
		/// Version of entity capabilities of sender.
		/// Entity capabilities are defined in XEP-0115: http://xmpp.org/extensions/xep-0115.html
		/// </summary>
		public string EntityCapabilityVersion => this.entityCapabilityVersion;

		/// <summary>
		/// Node of entity capabilities of sender.
		/// Entity capabilities are defined in XEP-0115: http://xmpp.org/extensions/xep-0115.html
		/// </summary>
		public string EntityCapabilityNode => this.entityCapabilityNode;

		/// <summary>
		/// Hash function used in calculation of entity capabilities version of sender.
		/// Entity capabilities are defined in XEP-0115: http://xmpp.org/extensions/xep-0115.html
		/// </summary>
		public string EntityCapabilityHashFunction => this.entityCapabilityHashFunction;

		/// <summary>
		/// If the presence stanza includes entity capabilities information.
		/// Entity capabilities are defined in XEP-0115: http://xmpp.org/extensions/xep-0115.html
		/// </summary>
		public bool HasEntityCapabilities => this.hasEntityCapabilities;

		/// <summary>
		/// Accepts a subscription or unsubscription request.
		/// </summary>
		/// <exception cref="Exception">If the presence element is not a subscription or unsubscription request.</exception>
		public void Accept()
		{
			if (this.type == PresenceType.Subscribe)
			{
				if (!(this.client is null))
					this.client.PresenceSubscriptionAccepted(this.id, this.fromBareJid);
				else
					this.component.PresenceSubscriptionAccepted(this.id, this.to, this.fromBareJid);
			}
			else if (this.type == PresenceType.Unsubscribe)
			{
				if (!(this.client is null))
					this.client.PresenceUnsubscriptionAccepted(this.id, this.fromBareJid);
				else
					this.component.PresenceUnsubscriptionAccepted(this.id, this.to, this.fromBareJid);
			}
			else
				throw new Exception("Presence stanza is not a subscription or unsubscription.");
		}

		/// <summary>
		/// Declines a subscription or unsubscription request.
		/// </summary>
		/// <exception cref="Exception">If the presence element is not a subscription or unsubscription request.</exception>
		public void Decline()
		{
			if (this.type == PresenceType.Subscribe)
			{
				if (!(this.client is null))
					this.client.PresenceSubscriptionDeclined(this.id, this.fromBareJid);
				else
					this.component.PresenceSubscriptionDeclined(this.id, this.to, this.fromBareJid);
			}
			else if (this.type == PresenceType.Unsubscribe)
			{
				if (!(this.client is null))
					this.client.PresenceUnsubscriptionDeclined(this.id, this.fromBareJid);
				else
					this.component.PresenceUnsubscriptionDeclined(this.id, this.to, this.fromBareJid);
			}
			else
				throw new Exception("Presence stanza is not a subscription or unsubscription.");
		}

		/// <summary>
		/// NickName, if available, as defined in XEP-0172. Can be sent in presence subscription requests, as an informal way to
		/// let the recipient know who the sender is. If not found, null is returned.
		/// </summary>
		public virtual string NickName
		{
			get
			{
				string NickName = null;

				foreach (XmlNode N in this.presence.ChildNodes)
				{
					if (N.LocalName == "nick" && N.NamespaceURI == "http://jabber.org/protocol/nick")   // XEP-0172
					{
						NickName = N.InnerText.Trim();
						break;
					}
				}

				return NickName;
			}
		}
	}
}
