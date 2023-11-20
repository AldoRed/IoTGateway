﻿using System.Xml;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Attributes
{
	/// <summary>
	/// String attribute
	/// </summary>
	public class StringAttribute : Attribute<string>
	{
		/// <summary>
		/// String attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public StringAttribute(string AttributeName, string Value, Layout2DDocument Document)
			: base(AttributeName, Value, Document)
		{
		}

		/// <summary>
		/// String attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public StringAttribute(XmlElement E, string AttributeName, Layout2DDocument Document)
			: base(E, AttributeName, true, Document)
		{
		}

		/// <summary>
		/// String attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public StringAttribute(string AttributeName, Expression Expression, Layout2DDocument Document)
			: base(AttributeName, Expression, Document)
		{
		}

		/// <summary>
		/// Tries to convert script result to a value of type <see cref="float"/>.
		/// </summary>
		/// <param name="Result">Script result.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvert(object Result, out string Value)
		{
			Value = Result?.ToString() ?? string.Empty;
			return true;
		}

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public override bool TryParse(string StringValue, out string Value)
		{
			Value = StringValue;
			return true;
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(string Value)
		{
			return Value;
		}

		/// <summary>
		/// Copies the attribute object if undefined, or defined by an expression.
		/// Returns a reference to itself, if preset (set by a constant value).
		/// </summary>
		/// <param name="ForDocument">Document that will host the new attribute.</param>
		/// <returns>Attribute reference.</returns>
		public StringAttribute CopyIfNotPreset(Layout2DDocument ForDocument)
		{
			if (this.HasPresetValue)
				return this;
			else
				return new StringAttribute(this.Name, this.Expression, ForDocument);
		}
	}
}
