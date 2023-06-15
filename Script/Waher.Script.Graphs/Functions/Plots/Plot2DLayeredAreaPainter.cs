﻿using System;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Graphs.Functions.Plots
{
	/// <summary>
	/// Plots a two-dimensional layered area chart.
	/// https://en.wikipedia.org/wiki/Area_chart
	/// </summary>
	public class Plot2DLayeredAreaPainter : SingleColorGraphPainter, IPainter2D
	{
		/// <summary>
		/// Draws the graph on a canvas.
		/// </summary>
		/// <param name="Canvas">Canvas to draw on.</param>
		/// <param name="Points">Points to draw.</param>
		/// <param name="Parameters">Graph-specific parameters.</param>
		/// <param name="PrevPoints">Points of previous graph of same type (if available), null (if not available).</param>
		/// <param name="PrevParameters">Parameters of previous graph of same type (if available), null (if not available).</param>
		/// <param name="DrawingArea">Current drawing area.</param>
		public void DrawGraph(SKCanvas Canvas, SKPoint[] Points, object[] Parameters, SKPoint[] PrevPoints, object[] PrevParameters,
			DrawingArea DrawingArea)
		{
			SKPaint Brush = null;
			SKPath Path = null;
			bool First = true;

			try
			{
				Brush = new SKPaint()
				{
					Style = SKPaintStyle.Fill,
					Color = Graph.ToColor(Parameters[0])
				};
				Path = new SKPath();

				foreach (SKPoint Point in Points)
				{
					if (First)
					{
						First = false;
						Path.MoveTo(Point);
					}
					else
						Path.LineTo(Point);
				}

				IElement Zero;
				ISet Set = DrawingArea.MinY.AssociatedSet;

				if (Set is IGroup Group)
					Zero = Group.AdditiveIdentity;
				else
					Zero = new DoubleNumber(0);

				IVector XAxis = VectorDefinition.Encapsulate(new IElement[] { DrawingArea.MinX, DrawingArea.MaxX }, false, null) as IVector;
				IVector YAxis = VectorDefinition.Encapsulate(new IElement[] { Zero, Zero }, false, null) as IVector;

				PrevPoints = DrawingArea.Scale(XAxis, YAxis);

				if (DrawingArea.MinX.AssociatedObjectValue is string && 
					DrawingArea.MaxX.AssociatedObjectValue is string)
				{
					PrevPoints[0].X = Points[0].X;
					PrevPoints[1].X = Points[Points.Length - 1].X;
				}

				int i = PrevPoints.Length;

				while (--i >= 0)
					Path.LineTo(PrevPoints[i]);

				Path.LineTo(Points[0]);

				Canvas.DrawPath(Path, Brush);
			}
			finally
			{
				Brush?.Dispose();
				Path?.Dispose();
			}
		}

	}
}
