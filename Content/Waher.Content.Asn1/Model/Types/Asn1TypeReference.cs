﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Macro;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// Represents an ASN.1 type reference.
	/// </summary>
	public class Asn1TypeReference : Asn1Type
	{
		private readonly Asn1Document document;
		private readonly string identifier;

		/// <summary>
		/// Represents an ASN.1 type reference.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Document">ASN.1 Document containing the reference</param>
		public Asn1TypeReference(string Identifier, Asn1Document Document)
			: base()
		{
			this.identifier = Identifier;
			this.document = Document;
		}

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => identifier;

		/// <summary>
		/// ASN.1 Document in where the value is defined.
		/// </summary>
		public Asn1Document Document => this.document;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override async Task ExportCSharp(StringBuilder Output, CSharpExportState State, int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
			{
				if (this.document.aliases.TryGetValue(this.identifier, out Asn1TypeDefinition TypeDef))
					await TypeDef.Definition.ExportCSharp(Output, State, Indent, Pass);
				else
					Output.Append(ToCSharp(this.identifier));
			}
		}

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			if (!(Macro.Document.namedNodes.TryGetValue(this.identifier, out Asn1Node Node)))
				throw Document.SyntaxError("Type named " + this.identifier + " not found.");

			if (Node is Asn1Type Type)
				return Type.Parse(Document, Macro);
			else if (Node is Asn1FieldDefinition FieldDef)
				return FieldDef.Type.Parse(Document, Macro);

			throw Document.SyntaxError("Type expected.");
		}
	}
}
