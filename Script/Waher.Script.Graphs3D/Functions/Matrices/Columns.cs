﻿using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Graphs3D.Functions.Matrices
{
	/// <summary>
	/// Creates a matrix whose columns have elements of the same value, each defined by 
	/// the corresponding element in the input vector.
	/// </summary>
	public class Columns : FunctionOneVectorVariable
	{
		/// <summary>
		/// Creates a matrix whose columns have elements of the same value, each defined by 
		/// the corresponding element in the input vector.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Columns(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Columns);

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			if (Argument is IMatrix M)
				return new DoubleNumber(M.Columns);
			else
				return base.Evaluate(Argument, Variables);
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(DoubleVector Argument, Variables Variables)
		{
			double[] Values = Argument.Values;
			int c = Values.Length;
			int i, j;
			double[,] Elements = new double[c, c];
			double Value;

			for (i = 0; i < c; i++)
			{
				Value = Values[i];
				for (j = 0; j < c; j++)
					Elements[i, j] = Value;
			}

			return new DoubleMatrix(Elements);
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(ComplexVector Argument, Variables Variables)
		{
			Complex[] Values = Argument.Values;
			int c = Values.Length;
			int i, j;
			Complex[,] Elements = new Complex[c, c];
			Complex Value;

			for (i = 0; i < c; i++)
			{
				Value = Values[i];
				for (j = 0; j < c; j++)
					Elements[i, j] = Value;
			}

			return new ComplexMatrix(Elements);
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(BooleanVector Argument, Variables Variables)
		{
			Boolean[] Values = Argument.Values;
			int c = Values.Length;
			int i, j;
			Boolean[,] Elements = new Boolean[c, c];
			Boolean Value;

			for (i = 0; i < c; i++)
			{
				Value = Values[i];
				for (j = 0; j < c; j++)
					Elements[i, j] = Value;
			}

			return new BooleanMatrix(Elements);
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(IVector Argument, Variables Variables)
		{
			int c = Argument.Dimension;
			int i, j;
			IElement[,] Elements = new IElement[c, c];

			for (i = 0; i < c; i++)
			{
				j = 0;
				foreach (IElement Value in Argument.VectorElements)
					Elements[i, j++] = Value;
			}

			return new ObjectMatrix(Elements);
		}

	}
}
