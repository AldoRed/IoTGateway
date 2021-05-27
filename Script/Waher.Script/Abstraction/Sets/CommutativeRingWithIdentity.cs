﻿using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Base class for all types of commutative rings with identity.
	/// </summary>
	public abstract class CommutativeRingWithIdentity : CommutativeRing, ICommutativeRingWithIdentity
	{
		/// <summary>
		/// Base class for all types of commutative rings with identity.
		/// </summary>
		public CommutativeRingWithIdentity()
			: base()
		{
		}

		/// <summary>
		/// Returns the identity element of the commutative ring with identity.
		/// </summary>
		public abstract ICommutativeRingWithIdentityElement One
		{
			get;
		}

	}
}
