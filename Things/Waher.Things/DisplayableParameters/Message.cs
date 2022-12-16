﻿using System;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Things.DisplayableParameters
{
	/// <summary>
	/// Type of message.
	/// </summary>
	public enum MessageType
	{
		/// <summary>
		/// Information message.
		/// </summary>
		Information,

		/// <summary>
		/// Warning message.
		/// </summary>
		Warning,

		/// <summary>
		/// Error message.
		/// </summary>
		Error
	}

	/// <summary>
	/// Contains information about a message logged on a node.
	/// </summary>
	public class Message
	{
		private DateTime timestamp;
		private MessageType type;
		private string eventId;
		private string body;

		/// <summary>
		/// Contains information about a message logged on a node.
		/// </summary>
		public Message()
		{
			this.timestamp = DateTime.MinValue;
			this.type = MessageType.Information;
			this.eventId = null;
			this.body = null;
		}

		/// <summary>
		/// Contains information about a message logged on a node.
		/// </summary>
		/// <param name="Timestamp">Message Timestamp.</param>
		/// <param name="Type">Type of message.</param>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public Message(DateTime Timestamp, MessageType Type, string EventId, string Body)
		{
			this.timestamp = Timestamp;
			this.type = Type;
			this.eventId = EventId;
			this.body = Body;
		}

		/// <summary>
		/// Timestamp
		/// </summary>
		public DateTime Timestamp
		{
			get => this.timestamp;
			set => this.timestamp = value;
		}

		/// <summary>
		/// Message Type
		/// </summary>
		public MessageType Type
		{
			get => this.type;
			set => this.type = value;
		}

		/// <summary>
		/// Optional Event ID.
		/// </summary>
		public string EventId
		{
			get => this.eventId;
			set => this.eventId = value;
		}

		/// <summary>
		/// Message body.
		/// </summary>
		public string Body
		{
			get => this.body;
			set => this.body = value;
		}

		/// <summary>
		/// Exports the message to XML.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		public void Export(StringBuilder Xml)
		{
			Xml.Append("<message timestamp='");
			Xml.Append(XML.Encode(this.timestamp));
			Xml.Append("' type='");
			Xml.Append(this.type.ToString());

			if (!string.IsNullOrEmpty(this.eventId))
			{
				Xml.Append("' eventId='");
				Xml.Append(this.eventId);
			}

			Xml.Append("'>");
			Xml.Append(XML.Encode(this.body));
			Xml.Append("</message>");
		}
	}
}
