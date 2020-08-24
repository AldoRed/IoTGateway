﻿using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// A translation transform
	/// </summary>
	public class Margins : LayoutContainer
	{
		private LengthAttribute left;
		private LengthAttribute right;
		private LengthAttribute top;
		private LengthAttribute bottom;

		/// <summary>
		/// A translation transform
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Margins(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Margins";

		/// <summary>
		/// Left
		/// </summary>
		public LengthAttribute LeftAttribute
		{
			get => this.left;
			set => this.left = value;
		}

		/// <summary>
		/// Right
		/// </summary>
		public LengthAttribute RightAttribute
		{
			get => this.right;
			set => this.right = value;
		}

		/// <summary>
		/// Top
		/// </summary>
		public LengthAttribute TopAttribute
		{
			get => this.top;
			set => this.top = value;
		}

		/// <summary>
		/// Bottom
		/// </summary>
		public LengthAttribute BottomAttribute
		{
			get => this.bottom;
			set => this.bottom = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.left = new LengthAttribute(Input, "left");
			this.right = new LengthAttribute(Input, "right");
			this.top = new LengthAttribute(Input, "top");
			this.bottom = new LengthAttribute(Input, "bottom");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.left?.Export(Output);
			this.right?.Export(Output);
			this.top?.Export(Output);
			this.bottom?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Margins(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Margins Dest)
			{
				Dest.left = this.left?.CopyIfNotPreset();
				Dest.right = this.right?.CopyIfNotPreset();
				Dest.top = this.top?.CopyIfNotPreset();
				Dest.bottom = this.bottom?.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool MeasureDimensions(DrawingState State)
		{
			this.Width = this.Height = this.Left = this.Top = this.Right = this.Bottom = null;

			bool Relative = false;

			if (!(this.left is null) && this.left.TryEvaluate(State.Session, out Length L))
				State.CalcDrawingSize(L, ref this.leftMargin, true, ref Relative);
			else
				this.leftMargin = 0;

			if (!(this.right is null) && this.right.TryEvaluate(State.Session, out L))
				State.CalcDrawingSize(L, ref this.rightMargin, true, ref Relative);
			else
				this.rightMargin = 0;

			if (!(this.top is null) && this.top.TryEvaluate(State.Session, out L))
				State.CalcDrawingSize(L, ref this.topMargin, true, ref Relative);
			else
				this.topMargin = 0;

			if (!(this.bottom is null) && this.bottom.TryEvaluate(State.Session, out L))
				State.CalcDrawingSize(L, ref this.bottomMargin, true, ref Relative);
			else
				this.bottomMargin = 0;

			SKSize Prev = State.SetAreaSize(new SKSize(
				State.AreaWidth - this.leftMargin - this.rightMargin,
				State.AreaHeight - this.topMargin - this.bottomMargin));

			if (base.MeasureDimensions(State))
				Relative = true;

			State.SetAreaSize(Prev);

			this.Width += this.leftMargin + this.rightMargin;
			this.Height += this.topMargin + this.bottomMargin;

			return Relative;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			SKSize Prev = State.SetAreaSize(new SKSize(
				State.AreaWidth - this.leftMargin - this.rightMargin,
				State.AreaHeight - this.topMargin - this.bottomMargin));

			base.MeasurePositions(State);

			State.SetAreaSize(Prev);
		}

		private float leftMargin;
		private float rightMargin;
		private float topMargin;
		private float bottomMargin;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			SKMatrix M = State.Canvas.TotalMatrix;
			State.Canvas.Translate(this.leftMargin, this.topMargin);

			base.Draw(State);

			State.Canvas.SetMatrix(M);
		}
	}
}
