﻿using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Lays out cells in a grid.
	/// </summary>
	public class GridCells : ICellLayout
	{
		private readonly List<GridPadding> elements = new List<GridPadding>();
		private readonly Variables session;
		private readonly int nrColumns;
		private readonly double?[] rights;
		private readonly List<double?> bottoms;
		private int x = 0;
		private int y = 0;

		/// <summary>
		/// Lays out cells in a grid.
		/// </summary>
		/// <param name="Session">Current session</param>
		/// <param name="NrColumns">Number of columns in grid</param>
		public GridCells(Variables Session, int NrColumns)
		{
			this.session = Session;
			this.nrColumns = NrColumns;
			this.rights = new double?[this.nrColumns];
			this.bottoms = new List<double?>();
		}

		private GridPadding GetElement(int X, int Y)
		{
			if (X < 0 || X >= this.nrColumns)
				return null;

			if (Y < 0)
				return null;

			int i = Y * this.nrColumns + X;
			if (i >= this.elements.Count)
				return null;

			return this.elements[i];
		}

		private void IncPos()
		{
			this.x++;
			if (this.x >= this.nrColumns)
			{
				this.x = 0;
				this.y++;
			}
		}

		private double GetRight(int x)
		{
			double? Result = 0;

			while (x >= 0 && !(Result = this.rights[x]).HasValue)
				x--;

			if (x < 0)
				return 0;
			else
				return Result.Value;
		}

		private double GetBottom(int y)
		{
			double? Result = 0;

			while (this.bottoms.Count <= this.y)
				this.bottoms.Add(null);

			while (y >= 0 && !(Result = this.bottoms[y]).HasValue)
				y--;

			if (y < 0)
				return 0;
			else
				return Result.Value;
		}

		/// <summary>
		/// Adds a cell to the layout.
		/// </summary>
		/// <param name="Element">Cell element</param>
		public void Add(ILayoutElement Element)
		{
			int ColSpan;
			int RowSpan;

			if (Element is Cell Cell)
				Cell.CalcSpan(this.session, out ColSpan, out RowSpan);
			else
				ColSpan = RowSpan = 1;

			while (!(this.GetElement(this.x, this.y) is null))
				this.IncPos();

			this.SetElement(this.x, this.y, ColSpan, RowSpan, Element);

			this.x += ColSpan;
			if (this.x >= this.nrColumns)
				this.x = this.nrColumns - 1;

			double Width = Element.Right - Element.Left;
			double Right = this.GetRight(this.x - 1) + Width;
			double Right2 = this.GetRight(this.x);
			if (Right > Right2)
				this.rights[this.x] = Right;

			this.y += RowSpan;

			double Height = Element.Bottom - Element.Top;
			double Bottom = this.GetBottom(this.y - 1) + Height;
			double Bottom2 = this.GetBottom(this.y);
			if (Bottom > Bottom2)
				this.bottoms[this.y] = Bottom;
		}

		private void SetElement(int X, int Y, int ColSpan, int RowSpan, ILayoutElement Element)
		{
			GridPadding P = new GridPadding(Element, -Element.Left, -Element.Top, ColSpan, RowSpan);

			if (ColSpan > 1 || RowSpan > 1)
			{
				int i;
				bool First = true;

				for (; RowSpan > 0; RowSpan--)
				{
					for (i = 0; i < ColSpan; i++)
					{
						this.SetElement(X + i, Y, P);

						if (First)
						{
							First = false;
							P = new GridPadding(null, 0, 0, 0, 0);
						}
					}

					Y++;
				}
			}
			else
				this.SetElement(X, Y, P);
		}

		private void SetElement(int X, int Y, GridPadding Element)
		{
			if (X < 0 || X >= this.nrColumns)
				return;

			if (Y < 0)
				return;

			int i = Y * this.nrColumns + X;
			int c = this.elements.Count;

			while (i > c)
			{
				this.elements.Add(null);
				c++;
			}

			if (i == c)
				this.elements.Add(Element);
			else
				this.elements[i] = Element;
		}

		/// <summary>
		/// Total width of layout
		/// </summary>
		public double TotWidth => this.GetRight(this.nrColumns - 1);

		/// <summary>
		/// Total height of layout
		/// </summary>
		public double TotHeight => this.GetBottom(this.bottoms.Count - 1);

		/// <summary>
		/// Aligns cells and returns an array of padded cells.
		/// </summary>
		/// <returns>Array of padded cells.</returns>
		public Padding[] Align()
		{
			List<Padding> Result = new List<Padding>();
			int X = 0;
			int Y = 0;

			foreach (GridPadding P in this.elements)
			{
				if (!(P.Element is null))
				{
					double Left = this.GetRight(X + P.ColSpan - 1);
					double Top = this.GetBottom(Y + P.RowSpan - 1);
					double Width = P.Element.Right - P.Element.Left;
					double Height = P.Element.Bottom - P.Element.Top;
					double MaxWidth = Left - this.GetRight(X - 1);
					double MaxHeight = Top - this.GetBottom(Y - 1);

					P.OffsetX += Left;
					P.OffsetY += Top;
					P.AlignedMeasuredCell(MaxWidth, MaxHeight, this.session);
					Result.Add(P);
				}

				X++;
				if (X >= this.nrColumns)
				{
					X = 0;
					Y++;
				}
			}

			return Result.ToArray();
		}
	}
}
