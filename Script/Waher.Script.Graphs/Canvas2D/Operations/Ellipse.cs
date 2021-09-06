﻿using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Draws an ellipse
	/// </summary>
	public class Ellipse : TwoCoordinates
	{
		/// <summary>
		/// Draws an ellipse
		/// </summary>
		public Ellipse()
			: base()
		{
		}

		/// <summary>
		/// Draws an ellipse
		/// </summary>
		/// <param name="X1">First X-coordinate.</param>
		/// <param name="Y1">First Y-coordinate.</param>
		/// <param name="X2">Second X-coordinate.</param>
		/// <param name="Y2">Second Y-coordinate.</param>
		public Ellipse(float X1, float Y1, float X2, float Y2)
			: base(X1, Y1, X2, Y2)
		{
		}

		/// <inheritdoc/>
		public override void Draw(SKCanvas Canvas, CanvasState State)
		{
			Canvas.DrawOval(new SKRect(this.X, this.Y, this.X2, this.Y2), State.Pen);
			State.X = this.X2;
			State.Y = this.Y2;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is Ellipse Obj && base.Equals(Obj));
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.GetType().FullName.GetHashCode();
			Result ^= Result << 5 ^ base.GetHashCode();
			return Result;
		}
	}
}
