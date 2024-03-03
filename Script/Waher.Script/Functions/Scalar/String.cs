﻿using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
	/// <summary>
	/// String(x)
	/// </summary>
	public class String : FunctionOneScalarStringVariable
	{
		/// <summary>
		/// String(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public String(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(String);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases
		{
			get { return new string[] { "str" }; }
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return new StringValue(Argument);
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (CheckAgainst is StringValue)
				return this.Argument.PatternMatch(CheckAgainst, AlreadyFound);
			else if (CheckAgainst.AssociatedObjectValue is string s)
				return this.Argument.PatternMatch(new StringValue(s), AlreadyFound);
			else if (CheckAgainst.AssociatedObjectValue is null)
				return this.Argument.PatternMatch(new StringValue(null), AlreadyFound);
			else
				return this.Argument.PatternMatch(new StringValue(Expression.ToString(CheckAgainst.AssociatedObjectValue)), AlreadyFound);
		}
	}
}
