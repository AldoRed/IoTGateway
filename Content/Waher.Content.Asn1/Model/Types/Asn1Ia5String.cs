﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// IA5String
	/// International ASCII characters (International Alphabet 5)
	/// </summary>
	public class Asn1Ia5String : Asn1StringType
	{
		/// <summary>
		/// IA5String
		/// International ASCII characters (International Alphabet 5)
		/// </summary>
		/// <param name="Implicit">Implicit type definition</param>
		public Asn1Ia5String(bool Implicit)
			: base(Implicit)
		{
		}
	}
}
