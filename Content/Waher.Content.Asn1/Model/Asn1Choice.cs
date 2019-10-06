﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents a ASN.1 CHOICE construct.
	/// </summary>
	public class Asn1Choice : Asn1List
	{
		/// <summary>
		/// Represents a ASN.1 CHOICE construct.
		/// </summary>
		/// <param name="FieldName">Optional field name.</param>
		/// <param name="Nodes">Nodes</param>
		public Asn1Choice(string FieldName, Asn1Node[] Nodes)
			: base(FieldName, Nodes)
		{
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => this.FieldName + "Choice";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => true;

		/// <summary>
		/// Exports implicit definitions to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		public override void ExportImplicitCSharp(StringBuilder Output, CSharpExportSettings Settings, int Indent)
		{
			foreach (Asn1Node Node in this.Nodes)
				Node.ExportImplicitCSharp(Output, Settings, Indent);


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
				if (Node is Asn1FieldDefinition Field)
				{
					if (First)
						First = false;
					else
						Output.Append(',');

					Output.AppendLine();
					Output.Append(Tabs(Indent));
					Output.Append(Field.FieldName);
				}
			}

			Indent--;

			Output.AppendLine();
			Output.Append(Tabs(Indent));
			Output.AppendLine("}");
			Output.AppendLine();


			Output.Append(Tabs(Indent));
			Output.Append("public class ");
			Output.Append(this.FieldName);
			Output.AppendLine("Choice");

			Output.Append(Tabs(Indent));
			Output.AppendLine("{");

			Indent++;

			Output.Append(Tabs(Indent));
			Output.Append(this.FieldName);
			Output.AppendLine("Enum _choice;");

			foreach (Asn1Node Node in this.Nodes)
				Node.ExportCSharp(Output, Settings, Indent);

			Indent--;

			Output.Append(Tabs(Indent));
			Output.AppendLine("}");
			Output.AppendLine();
		}
	}
}
