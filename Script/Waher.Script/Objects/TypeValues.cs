﻿using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Set of Type values.
	/// </summary>
	public sealed class TypeValues : Set, IOrderedSet
	{
		private static readonly int hashCode = typeof(TypeValues).FullName.GetHashCode();

		/// <summary>
		/// Set of Type values.
		/// </summary>
		public TypeValues()
		{
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(IElement Element)
		{
			return Element is TypeValue;
		}

		/// <summary>
		/// <see cref="Type.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is TypeValues;
		}

		/// <summary>
		/// <see cref="Type.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return hashCode;
		}

		/// <summary>
		/// Compares two Type values.
		/// </summary>
		/// <param name="x">Value 1</param>
		/// <param name="y">Value 2</param>
		/// <returns>Result</returns>
		public int Compare(IElement x, IElement y)
		{
			TypeValue o1 = (TypeValue)x;
			TypeValue o2 = (TypeValue)y;

			if (!(o1.Value is IComparable c1) || !(o2.Value is IComparable c2))
				throw new ScriptException("Values not comparable.");

			return c1.CompareTo(c2);
		}
	}
}
