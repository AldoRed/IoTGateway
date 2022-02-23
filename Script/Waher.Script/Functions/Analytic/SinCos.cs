﻿using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Analytic
{
	/// <summary>
	/// SinCos(x), calculates both Sin(x) and Cos(x)
	/// </summary>
	public class SinCos : FunctionOneScalarVariable
	{
		/// <summary>
		/// SinCos(x), calculates both Sin(x) and Cos(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SinCos(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "SinCos";

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument, Variables Variables)
		{
			return new DoubleVector(
				Math.Sin(Argument),
				Math.Cos(Argument));
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(Complex Argument, Variables Variables)
		{
			return new ComplexVector(
				Complex.Sin(Argument),
				Complex.Cos(Argument));
		}
	}
}
