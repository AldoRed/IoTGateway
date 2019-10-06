﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for ASN.1 types.
	/// </summary>
	public abstract class Asn1Type : Asn1Node
	{
		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="TypeName">Type name.</param>
		public virtual void ExportCSharpTypeDefinition(StringBuilder Output, CSharpExportSettings Settings, 
			int Indent, string TypeName)
		{
			throw new NotImplementedException("Support for exporting type definitions of type " +
				this.GetType().FullName + " not implemented.");
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public virtual string CSharpTypeReference
		{
			get
			{
				throw new NotImplementedException("Support for exporting type references of type " +
					this.GetType().FullName + " not implemented.");
			}
		}

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public virtual bool CSharpTypeNullable
		{
			get
			{
				throw new NotImplementedException("Nullable information for type " +
					this.GetType().FullName + " not implemented.");
			}
		}
	}
}
