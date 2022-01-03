﻿using System;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Unit of length
	/// </summary>
	public enum LengthUnit
	{
		/// <summary>
		/// pixels (1px = 1/96th of 1in) (absolute)
		/// </summary>
		Px,

		/// <summary>
		/// points (1pt = 1/72 of 1in) (absolute)
		/// </summary>
		Pt,

		/// <summary>
		/// picas (1pc = 12 pt) (absolute)
		/// </summary>
		Pc,

		/// <summary>
		/// centimeters (absolute)
		/// </summary>
		Cm,

		/// <summary>
		/// inches (1in = 96px = 2.54cm)
		/// </summary>
		In,

		/// <summary>
		/// millimeters (absolute)
		/// </summary>
		Mm,

		/// <summary>
		/// Relative to the font-size of the element (2em means 2 times the size of the current font)
		/// </summary>
		Em,

		/// <summary>
		/// Relative to the x-height of the current font (rarely used)
		/// </summary>
		Ex,

		/// <summary>
		/// Relative to the width of the "0" (zero)
		/// </summary>
		Ch,

		/// <summary>
		/// Relative to font-size of the root element
		/// </summary>
		Rem,

		/// <summary>
		/// Relative to 1% of the width of the viewport
		/// </summary>
		Vw,

		/// <summary>
		/// Relative to 1% of the height of the viewport
		/// </summary>
		Vh,

		/// <summary>
		/// Relative to 1% of viewport's smaller dimension
		/// </summary>
		Vmin,

		/// <summary>
		/// Relative to 1% of viewport's larger dimension
		/// </summary>
		Vmax,

		/// <summary>
		/// Relative to the parent element
		/// </summary>
		Percent
	}

	/// <summary>
	/// Length definition.
	/// </summary>
	public class Length
	{
		/// <summary>
		/// Length definition.
		/// </summary>
		/// <param name="Value">Value of length</param>
		/// <param name="Unit">Unit of length.</param>
		public Length(float Value, LengthUnit Unit)
		{
			this.Value = Value;
			this.Unit = Unit;
		}

		/// <summary>
		/// Value of length
		/// </summary>
		public float Value
		{
			get;
			internal set;
		}

		/// <summary>
		/// Unit of length.
		/// </summary>
		public LengthUnit Unit
		{
			get;
			internal set;
		}

		/// <summary>
		/// 100%
		/// </summary>
		public static readonly Length HundredPercent = new Length(100, LengthUnit.Percent);

		/// <inheritdoc/>
		public override string ToString()
		{
			if (this.Unit == LengthUnit.Percent)
				return this.Value.ToString() + "%";
			else
				return this.Value.ToString() + " " + this.Unit.ToString().ToLower();
		}
	}
}
