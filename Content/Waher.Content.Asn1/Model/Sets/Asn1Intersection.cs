﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Intersection of sets.
	/// </summary>
	public class Asn1Intersection : Asn1BinaryOperator
	{
		/// <summary>
		/// Union of sets.
		/// </summary>
		/// <param name="Left">Left set</param>
		/// <param name="Right">Right set</param>
		public Asn1Intersection(Asn1Set Left, Asn1Set Right)
			: base(Left, Right)
		{
		}
	}
}
