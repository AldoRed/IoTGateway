﻿using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Functions.Matrices
{
	/// <summary>
	/// Creates a matrix containing only ones.
	/// </summary>
	public class Ones : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Creates a matrix containing only ones.
		/// </summary>
		/// <param name="Rows">Rows of the resulting matrix.</param>
		/// <param name="Columns">Columns of the resulting matrix.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Ones(ScriptNode Rows, ScriptNode Columns, int Start, int Length, Expression Expression)
			: base(Rows, Columns, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Ones);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Rows", "Columns" };

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument1, double Argument2, Variables Variables)
		{
			int Rows = (int)Argument1;
			if (Rows != Argument1 || Rows <= 0)
				throw new ScriptRuntimeException("Rows must be a positive integer.", this);

			int Columns = (int)Argument2;
			if (Columns != Argument2 || Columns <= 0)
				throw new ScriptRuntimeException("Columns must be a positive integer.", this);

			double[,] E = new double[Rows, Columns];
			int i, j;

			for (i = 0; i < Rows; i++)
			{
				for (j = 0; j < Columns; j++)
					E[i, j] = 1;
			}

			return new DoubleMatrix(E);
		}
	}
}
