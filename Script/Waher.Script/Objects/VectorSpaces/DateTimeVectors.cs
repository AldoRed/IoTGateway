﻿using System;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;

namespace Waher.Script.Objects.VectorSpaces
{
	/// <summary>
	/// Pseudo-vector space of DateTime-valued vectors.
	/// </summary>
	public sealed class DateTimeVectors : VectorSpace
	{
		private readonly int dimension;

		/// <summary>
		/// Pseudo-vector space of DateTime-valued vectors.
		/// </summary>
		/// <param name="Dimension">Dimension.</param>
		public DateTimeVectors(int Dimension)
		{
			this.dimension = Dimension;
		}

		/// <summary>
		/// Dimension of vector space.
		/// </summary>
		public int Dimension => this.dimension;

		/// <summary>
		/// Scalar field.
		/// </summary>
		public override IField ScalarField
		{
			get { throw new ScriptException("Scalar field not defined for generic object vectors."); }
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get { throw new ScriptException("Zero element not defined for generic object vectors."); }
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			if (Element is DateTimeVector v)
				return v.Dimension == this.dimension;
			else
				return false;
		}

		/// <summary>
		/// Compares the element to another.
		/// </summary>
		/// <param name="obj">Other element to compare against.</param>
		/// <returns>If elements are equal.</returns>
		public override bool Equals(object obj)
		{
			return (obj is DateTimeVectors v && v.dimension == this.dimension);
		}

		/// <summary>
		/// Calculates a hash code of the element.
		/// </summary>
		/// <returns>Hash code.</returns>
		public override int GetHashCode()
		{
			return this.dimension.GetHashCode();
		}

	}
}
