﻿using System;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// </summary>
    public abstract class PersonalEvent : IPersonalEvent
	{
		/// <summary>
		/// Node on which the personal event is published on.
		/// </summary>
		public virtual string Node => this.Namespace;

		/// <summary>
		/// Local name of the event element.
		/// </summary>
		public abstract string LocalName
		{
			get;
		}

		/// <summary>
		/// Namespace of the event element.
		/// </summary>
		public abstract string Namespace
		{
			get;
		}

		/// <summary>
		/// Optional Item ID of event. If null, a new will automatically be generated by the server.
		/// </summary>
		public virtual string ItemId
		{
			get { return null; }
		}

		/// <summary>
		/// XML representation of the event.
		/// </summary>
		public abstract string PayloadXml
		{
			get;
		}

		/// <summary>
		/// Parses a personal event from its XML representation
		/// </summary>
		/// <param name="E">XML representation of personal event.</param>
		/// <returns>Personal event object.</returns>
		public abstract IPersonalEvent Parse(XmlElement E);

		/// <summary>
		/// Serializes an optional decimal value.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="LocalName">Local name of element.</param>
		/// <param name="Value">Optional value.</param>
		protected void Append(StringBuilder Xml, string LocalName, decimal? Value)
		{
			if (Value.HasValue)
			{
				Xml.Append('<');
				Xml.Append(LocalName);
				Xml.Append('>');
				Xml.Append(CommonTypes.Encode(Value.Value));
				Xml.Append("</");
				Xml.Append(LocalName);
				Xml.Append('>');
			}
		}

		/// <summary>
		/// Serializes an optional date and time value.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="LocalName">Local name of element.</param>
		/// <param name="Value">Optional value.</param>
		protected void Append(StringBuilder Xml, string LocalName, DateTime? Value)
		{
			if (Value.HasValue)
			{
				Xml.Append('<');
				Xml.Append(LocalName);
				Xml.Append('>');
				Xml.Append(XML.Encode(Value.Value));
				Xml.Append("</");
				Xml.Append(LocalName);
				Xml.Append('>');
			}
		}

		/// <summary>
		/// Serializes an optional string value.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="LocalName">Local name of element.</param>
		/// <param name="Value">Optional value.</param>
		protected void Append(StringBuilder Xml, string LocalName, string Value)
		{
			if (!(Value is null))
			{
				Xml.Append('<');
				Xml.Append(LocalName);
				Xml.Append('>');
				Xml.Append(XML.Encode(Value));
				Xml.Append("</");
				Xml.Append(LocalName);
				Xml.Append('>');
			}
		}
	}
}
