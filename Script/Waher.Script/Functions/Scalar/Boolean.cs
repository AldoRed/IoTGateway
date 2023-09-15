﻿using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
    /// <summary>
    /// Boolean(x)
    /// </summary>
    public class Boolean : FunctionOneScalarVariable
    {
        /// <summary>
        /// Boolean(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Boolean(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Boolean);

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases
        {
            get { return new string[] { "bool" }; }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(double Argument, Variables Variables)
        {
            return new BooleanValue(Argument != 0);
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(Complex Argument, Variables Variables)
        {
            return new BooleanValue(Argument != Complex.Zero);
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(bool Argument, Variables Variables)
        {
            return new BooleanValue(Argument);
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
			bool? Value = ToBoolean(Argument.ToLower());

            if (Value.HasValue)
              return new BooleanValue(Value.Value);
			else
				throw new ScriptException("Not a boolean value.");
		}

        /// <summary>
        /// Converts a string to a boolean value, if possible.
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <returns>Boolean value, if converted, or null if not possible.</returns>
        public static bool? ToBoolean(string Value)
		{
            if (Value == "1" || Value == "true" || Value == "yes" || Value == "on")
                return true;
            else if (Value == "0" || Value == "false" || Value == "no" || Value == "off" || string.IsNullOrEmpty(Value))
                return false;
            else
                return null;
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
            return Task.FromResult(this.EvaluateScalar(Argument, Variables));
        }

        /// <summary>
        /// Performs a pattern match operation.
        /// </summary>
        /// <param name="CheckAgainst">Value to check against.</param>
        /// <param name="AlreadyFound">Variables already identified.</param>
        /// <returns>Pattern match result</returns>
        public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
        {
            if (CheckAgainst.AssociatedObjectValue is bool)
                return this.Argument.PatternMatch(CheckAgainst, AlreadyFound);
            else
            {
                bool? b = ToBoolean(CheckAgainst);

                if (b.HasValue)
                    return this.Argument.PatternMatch(new BooleanValue(b.Value), AlreadyFound);
                else
                    return PatternMatchResult.NoMatch;
            }
        }
    }
}
