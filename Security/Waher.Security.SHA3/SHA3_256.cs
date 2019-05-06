﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.SHA3
{
	/// <summary>
	/// Implements the SHA3-256 hash function, as defined in section 6.1
	/// in the NIST FIPS 202: 
	/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
	/// </summary>
	public class SHA3_256 : Keccak
	{
		/// <summary>
		/// Implements the SHA3-256 hash function, as defined in section 6.1
		/// in the NIST FIPS 202: 
		/// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
		/// </summary>
		public SHA3_256()
			: base(BitSize.BitSize1600, 512, 0b01, 256)
		{
		}
	}
}
