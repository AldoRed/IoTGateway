﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// The "redirect" modifier is intended for consolidating both
	/// authorizations and policy into a common set to be shared within a
	/// single ADMD.It is possible to control both authorized hosts and
	/// policy for an arbitrary number of domains from a single record.
	/// </summary>
	public class Redirect : MechanismDomainSpec
	{
		/// <summary>
		/// The "redirect" modifier is intended for consolidating both
		/// authorizations and policy into a common set to be shared within a
		/// single ADMD.It is possible to control both authorized hosts and
		/// policy for an arbitrary number of domains from a single record.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public Redirect(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}

		/// <summary>
		/// Mechanism separator
		/// </summary>
		public override char Separator => '=';
	}
}
