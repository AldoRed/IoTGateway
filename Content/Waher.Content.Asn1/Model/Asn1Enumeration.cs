﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// ENUMERATED
	/// </summary>
	public class Asn1Enumeration : Asn1List 
	{
		/// <summary>
		/// ENUMERATED
		/// </summary>
		/// <param name="FieldName">Optional field name.</param>
		/// <param name="Nodes">Nodes</param>
		public Asn1Enumeration(string FieldName, Asn1Node[] Nodes)
			: base(FieldName, Nodes)
		{
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => this.FieldName + "Enum";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => false;

		/// <summary>
		/// Exports implicit definitions to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		public override void ExportImplicitCSharp(StringBuilder Output, CSharpExportSettings Settings, int Indent)
		{
			Output.Append(Tabs(Indent));
			Output.Append("public enum ");
			Output.Append(this.FieldName);
			Output.AppendLine("Enum");

			Output.Append(Tabs(Indent));
			Output.Append("{");
			
			Indent++;

			bool First = true;

			foreach (Asn1Node Node in this.Nodes)
			{
				if (First)
					First = false;
				else
					Output.Append(',');

				Output.AppendLine();
				Output.Append(Tabs(Indent));
				Node.ExportCSharp(Output, Settings, Indent);
			}

			Indent--;

			Output.AppendLine();
			Output.Append(Tabs(Indent));
			Output.AppendLine("}");
			Output.AppendLine();
		}

	}
}
