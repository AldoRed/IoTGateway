﻿using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Script.Operators.Sets
{
	/// <summary>
	/// To-Set operator.
	/// </summary>
	public class ToSet : NullCheckUnaryOperator
	{
		/// <summary>
		/// To-Set operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ToSet(ScriptNode Operand, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Operand, NullCheck, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(IElement Operand, Variables Variables)
		{
			return this.ConvertToSet(Operand);
		}

		private IElement ConvertToSet(IElement Operand)
		{
			if (this.nullCheck && Operand.AssociatedObjectValue is null)
				return Operand;

			if (Operand is ISet)
				return Operand;

			if (Operand is IVector V)
				return SetDefinition.Encapsulate(V.VectorElements, this);

			if (this.nullCheck && Operand.AssociatedObjectValue is null)
				return Operand;

			return SetDefinition.Encapsulate(new IElement[] { Operand }, this);
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			return this.op.PatternMatch(this.ConvertToSet(CheckAgainst), AlreadyFound);
		}
	}
}
