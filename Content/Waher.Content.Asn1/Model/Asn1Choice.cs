﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;

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
		/// <param name="Name">Optional field or type name.</param>
		/// <param name="TypeDef">If construct is part of a type definition.</param>
		/// <param name="Nodes">Nodes</param>
		public Asn1Choice(string Name, bool TypeDef, Asn1Node[] Nodes)
			: base(Name, TypeDef, Nodes)
		{
		}

		/// <summary>
		/// If the type is a constructed type.
		/// </summary>
		public override bool ConstructedType => true;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			int Bak = Document.pos;

			foreach (Asn1Node Choice in this.Nodes)
			{
				if (Choice is Asn1Type Type)
				{
					try
					{
						Document.pos = Bak;
						return Type.Parse(Document, Macro);
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}

			throw Document.SyntaxError("Unable to parse choices.");
		}

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override async Task ExportCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit && !this.TypeDefinition)
			{
				Output.Append(ToCSharp(this.Name));
				Output.Append("Choice");
			}
			else
			{
				if (Pass == CSharpExportPass.Implicit)
				{
					State.ClosePending(Output);

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, Pass);

					Output.Append(Tabs(Indent));
					Output.Append("public enum ");
					Output.Append(ToCSharp(this.Name));
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
							Output.Append(Field.Name);
						}
					}

					Indent--;

					Output.AppendLine();
					Output.Append(Tabs(Indent));
					Output.AppendLine("}");
					Output.AppendLine();
				}

				if (Pass == CSharpExportPass.Implicit || Pass == CSharpExportPass.Explicit)
				{
					Output.Append(Tabs(Indent));
					Output.Append("public class ");
					Output.Append(ToCSharp(this.Name));
					if (!this.TypeDefinition)
						Output.Append("Choice");
					Output.AppendLine();

					Output.Append(Tabs(Indent));
					Output.AppendLine("{");

					Indent++;

					Output.Append(Tabs(Indent));
					Output.Append("public ");
					Output.Append(ToCSharp(this.Name));
					if (!this.TypeDefinition)
						Output.Append("Enum");
					Output.AppendLine(" _choice;");

					foreach (Asn1Node Node in this.Nodes)
						await Node.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

					Indent--;

					Output.Append(Tabs(Indent));
					Output.AppendLine("}");
					Output.AppendLine();
				}
			}
		}
	}
}
