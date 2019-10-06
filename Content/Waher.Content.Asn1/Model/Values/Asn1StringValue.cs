﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// String value
	/// </summary>
	public class Asn1StringValue : Asn1Value
	{
		private readonly string value;

		/// <summary>
		/// String value
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1StringValue(string Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public string Value => this.value;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportSettings Settings,
			int Indent)
		{
			Output.Append('"');
			Output.Append(this.value.Replace("\\", "\\\\").Replace("\r", "\\r").
				Replace("\n", "\\n").Replace("\t", "\\t").Replace("\a", "\\a").
				Replace("\b", "\\b").Replace("\f", "\\f").Replace("\"", "\\\"").
				Replace("'", "\\'"));
			Output.Append('"');
		}
	}
}
