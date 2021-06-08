﻿using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Draws a rectangle
	/// </summary>
	public class Rectangle : TwoCoordinates
	{
		/// <summary>
		/// Draws a rectangle
		/// </summary>
		public Rectangle()
			: base()
		{
		}

		/// <summary>
		/// Draws a rectangle
		/// </summary>
		/// <param name="X1">First X-coordinate.</param>
		/// <param name="Y1">First Y-coordinate.</param>
		/// <param name="X2">Second X-coordinate.</param>
		/// <param name="Y2">Second Y-coordinate.</param>
		public Rectangle(float X1, float Y1, float X2, float Y2)
			: base(X1, Y1, X2, Y2)
		{
		}

		/// <inheritdoc/>
		public override void Draw(SKCanvas Canvas, CanvasState State)
		{
			Canvas.DrawRect(this.X, this.Y, this.Width, this.Height, State.Pen);
			State.X = this.X2;
			State.Y = this.Y2;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is Line Obj && base.Equals(Obj));
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
