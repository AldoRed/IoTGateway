﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// CHARACTER
	/// </summary>
	public class Asn1Character : Asn1Type
	{
		/// <summary>
		/// CHARACTER
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1Character(bool Implicit)
			: base(Implicit)
		{
		}

		/// <summary>
		/// C# type reference.
		/// </summary>
		public override string CSharpTypeReference => "char";

		/// <summary>
		/// If type is nullable.
		/// </summary>
		public override bool CSharpTypeNullable => false;
	}
}
