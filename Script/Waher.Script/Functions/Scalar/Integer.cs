﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
	/// <summary>
	/// Integer(x)
	/// </summary>
	public class Integer : FunctionOneScalarVariable
	{
		/// <summary>
		/// Integer(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Integer(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Integer);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases
		{
			get { return new string[] { "int" }; }
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument, Variables Variables)
		{
			return new DoubleNumber(Math.Truncate(Argument));
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(bool Argument, Variables Variables)
		{
			return new DoubleNumber(Argument ? 1 : 0);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			if (!Expression.TryParse(Argument, out double d))
				throw new ScriptException("Not an integer.");

			return new DoubleNumber(Math.Truncate(d));
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			return this.EvaluateScalar(Argument.ToString(), Variables);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
		{
			return Task.FromResult<IElement>(this.EvaluateScalar(Argument, Variables));
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			object Obj = CheckAgainst.AssociatedObjectValue;
			
			if (!(Obj is BigInteger))
			{
				if (Obj is double d)
				{
					if (Math.Truncate(d) != d)
						return PatternMatchResult.NoMatch;
				}
				else if (Obj is string s)
				{
					if (int.TryParse(s, out int i))
						return this.Argument.PatternMatch(new DoubleNumber(i), AlreadyFound);
					else if (long.TryParse(s, out long l) && ((long)((double)l)) == l)
						return this.Argument.PatternMatch(new DoubleNumber(l), AlreadyFound);
					else if (BigInteger.TryParse(s, out BigInteger I))
						return this.Argument.PatternMatch(new Objects.Integer(I), AlreadyFound);
				}
				else
					return PatternMatchResult.NoMatch;
			}

			return this.Argument.PatternMatch(CheckAgainst, AlreadyFound);
		}
	}
}
