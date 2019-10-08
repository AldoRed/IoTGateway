﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// UniversalString
	/// ISO10646 character set
	/// </summary>
	public class Asn1UniversalString : Asn1StringType
	{
		/// <summary>
		/// UniversalString
		/// ISO10646 character set
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1UniversalString(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
