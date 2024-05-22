﻿using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Constants
{
    /// <summary>
    /// Complex numbers.
    /// </summary>
    public class C : IConstant
	{
        /// <summary>
        /// Complex numbers.
        /// </summary>
        public C()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName => nameof(C);

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases => new string[] { "ℂ" };

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			return set;
		}

		private static readonly ComplexNumbers set = new ComplexNumbers();
	}
}
