﻿using System;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Boolean contractual parameter
	/// </summary>
	public class BooleanParameter : Parameter
	{
		private bool? value;

		/// <summary>
		/// Parameter value
		/// </summary>
		public bool? Value
		{
			get => this.value;
			set => this.value = value;
		}

		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.value;

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<booleanParameter name=\"");
			Xml.Append(XML.Encode(this.Name));

			if (this.value.HasValue)
			{
				Xml.Append("\" value=\"");
				Xml.Append(CommonTypes.Encode(this.value.Value));
			}

			if (this.Descriptions is null || this.Descriptions.Length == 0)
				Xml.Append("\"/>");
			else
			{
				Xml.Append("\">");

				foreach (HumanReadableText Description in this.Descriptions)
					Description.Serialize(Xml, "description", false);

				Xml.Append("</booleanParameter>");
			}
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <returns>If parameter value is valid.</returns>
		public override bool IsParameterValid(Variables Variables)
		{
			if (!(this.value.HasValue))
				return false;

			return base.IsParameterValid(Variables);
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public override void Populate(Variables Variables)
		{
			Variables[this.Name] = this.value;
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			if (Value is bool b)
				this.value = b;
			else if (Value is string s && CommonTypes.TryParse(s, out b))
				this.value = b;
			else
				throw new ArgumentException("Invalid parameter type.", nameof(Value));
		}

	}
}
