﻿using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Statistics.Functions
{
	/// <summary>
	/// Computes a two-dimensional histogram from a set of data.
	/// </summary>
	public class Histogram2D : FunctionMultiVariate
	{
		private static readonly ArgumentType[] argumentTypes7Parameters = new ArgumentType[]
		{
			ArgumentType.Matrix,	// M
			ArgumentType.Scalar,	// MinX
			ArgumentType.Scalar,	// MaxX
			ArgumentType.Scalar,	// NX
			ArgumentType.Scalar,	// MinY
			ArgumentType.Scalar,	// MaxY
			ArgumentType.Scalar		// NY
		};

		/// <summary>
		/// Computes a two-dimensional histogram from a set of data.
		/// </summary>
		/// <param name="Data">Data</param>
		/// <param name="MinX">Smallest value along the X-axis.</param>
		/// <param name="MaxX">Largest value along the X-axis.</param>
		/// <param name="NX">Number of buckets along the X-axis.</param>
		/// <param name="MinY">Smallest value along the Y-axis.</param>
		/// <param name="MaxY">Largest value along the Y-axis.</param>
		/// <param name="NY">Number of buckets along the Y-axis.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Histogram2D(ScriptNode Data, ScriptNode MinX, ScriptNode MaxX, ScriptNode NX,
			ScriptNode MinY, ScriptNode MaxY, ScriptNode NY, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Data, MinX, MaxX, NX, MinY, MaxY, NY }, argumentTypes7Parameters, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Histogram2D);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "data", "minX", "maxX", "NX", "minY", "maxY", "NY" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			IMatrix Data = Arguments[0] as IMatrix ?? throw new ScriptRuntimeException("First argument must be a matrix.", this);
			double MinX = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			double MaxX = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
			double dX = Expression.ToDouble(Arguments[3].AssociatedObjectValue);
			int NX = (int)Math.Round(dX);
			if (NX <= 0 || NX != dX)
				throw new ScriptRuntimeException("NX must be a positive integer.", this);

			if (MaxX <= MinX)
				throw new ScriptRuntimeException("MinX must be smaller than MaxX.", this);

			double MinY = Expression.ToDouble(Arguments[4].AssociatedObjectValue);
			double MaxY = Expression.ToDouble(Arguments[5].AssociatedObjectValue);
			double dY = Expression.ToDouble(Arguments[6].AssociatedObjectValue);
			int NY = (int)Math.Round(dY);
			if (NY <= 0 || NY != dY)
				throw new ScriptRuntimeException("NY must be a positive integer.", this);

			if (MaxY <= MinY)
				throw new ScriptRuntimeException("MinY must be smaller than MaxY.", this);

			double[,] Result = new double[NX, NY];
			double DiffX = MaxX - MinX;
			double DiffY = MaxY - MinY;
			double x, y;
			int ix, iy, j, c;

			if (Data is DoubleMatrix M)
			{
				double[,] w = M.Values;

				if (w.GetLength(0) == 2)
				{
					c = w.GetLength(1);

					for (j = 0; j < c; j++)
					{
						x = w[0, j];
						y = w[1, j];

						if (x >= MinX && x <= MaxX && y >= MinY && y <= MaxY)
						{
							ix = (int)(((x - MinX) * NX) / DiffX);
							if (ix == NX)
								ix--;

							iy = (int)(((y - MinY) * NY) / DiffY);
							if (iy == NY)
								iy--;

							Result[ix, iy]++;
						}
					}
				}
				else if (w.GetLength(1) == 2)
				{
					c = w.GetLength(0);

					for (j = 0; j < c; j++)
					{
						x = w[j, 0];
						y = w[j, 1];

						if (x >= MinX && x <= MaxX && y >= MinY && y <= MaxY)
						{
							ix = (int)(((x - MinX) * NX) / DiffX);
							if (ix == NX)
								ix--;

							iy = (int)(((y - MinY) * NY) / DiffY);
							if (iy == NY)
								iy--;

							Result[ix, iy]++;
						}
					}
				}
				else
					throw new ScriptRuntimeException("Matrix must have either 2 columns or 2 rows.", this);
			}
			else
			{
				IElement E;

				if (Data.Rows == 2)
				{
					c = Data.Columns;

					for (j = 0; j < c; j++)
					{
						E = Data.GetElement(j, 0);

						if (E.AssociatedObjectValue is double x2)
							x = x2;
						else
						{
							try
							{
								x = Expression.ToDouble(E.AssociatedObjectValue);
							}
							catch (Exception)
							{
								continue;
							}
						}

						E = Data.GetElement(j, 1);

						if (E.AssociatedObjectValue is double y2)
							y = y2;
						else
						{
							try
							{
								y = Expression.ToDouble(E.AssociatedObjectValue);
							}
							catch (Exception)
							{
								continue;
							}
						}

						if (x >= MinX && x <= MaxX && y >= MinY && y <= MaxY)
						{
							ix = (int)(((x - MinX) * NX) / DiffX);
							if (ix == NX)
								ix--;

							iy = (int)(((y - MinY) * NY) / DiffY);
							if (iy == NY)
								iy--;

							Result[ix, iy]++;
						}
					}
				}
				else if (Data.Columns == 2)
				{
					c = Data.Rows;

					for (j = 0; j < c; j++)
					{
						E = Data.GetElement(0, j);

						if (E.AssociatedObjectValue is double x2)
							x = x2;
						else
						{
							try
							{
								x = Expression.ToDouble(E.AssociatedObjectValue);
							}
							catch (Exception)
							{
								continue;
							}
						}

						E = Data.GetElement(1, j);

						if (E.AssociatedObjectValue is double y2)
							y = y2;
						else
						{
							try
							{
								y = Expression.ToDouble(E.AssociatedObjectValue);
							}
							catch (Exception)
							{
								continue;
							}
						}

						if (x >= MinX && x <= MaxX && y >= MinY && y <= MaxY)
						{
							ix = (int)(((x - MinX) * NX) / DiffX);
							if (ix == NX)
								ix--;

							iy = (int)(((y - MinY) * NY) / DiffY);
							if (iy == NY)
								iy--;

							Result[ix, iy]++;
						}
					}
				}
				else
					throw new ScriptRuntimeException("Matrix must have either 2 columns or 2 rows.", this);
			}

			return new ObjectVector(new IElement[]
			{
				new ObjectVector(Histogram.GetLabels(NX, MinX, DiffX)),
				new ObjectVector(Histogram.GetLabels(NY, MinY, DiffY)),
				new DoubleMatrix(Result)
			});
		}

	}
}
