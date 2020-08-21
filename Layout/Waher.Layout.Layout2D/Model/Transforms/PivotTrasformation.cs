﻿using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Transforms
{
	/// <summary>
	/// Base abstract class for transformations using an optional pivot point.
	/// </summary>
	public abstract class PivotTrasformation : LinearTrasformation
	{
		private LengthAttribute x;
		private LengthAttribute y;
		private StringAttribute _ref;

		/// <summary>
		/// Base abstract class for transformations using an optional pivot point.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public PivotTrasformation(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// X-coordinate
		/// </summary>
		public LengthAttribute XAttribute
		{
			get => this.x;
			set => this.x = value;
		}

		/// <summary>
		/// Y-coordinate
		/// </summary>
		public LengthAttribute YAttribute
		{
			get => this.y;
			set => this.y = value;
		}

		/// <summary>
		/// Reference
		/// </summary>
		public StringAttribute ReferenceAttribute
		{
			get => this._ref;
			set => this._ref = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.x = new LengthAttribute(Input, "x");
			this.y = new LengthAttribute(Input, "y");
			this._ref = new StringAttribute(Input, "ref");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.x.Export(Output);
			this.y.Export(Output);
			this._ref.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is PivotTrasformation Dest)
			{
				Dest.x = this.x.CopyIfNotPreset();
				Dest.y = this.y.CopyIfNotPreset();
				Dest._ref = this._ref.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasureDimensions(DrawingState State)
		{
			base.MeasureDimensions(State);

			if (!this.CalcPoint(State, this.x, this.y, this._ref, out this.xCoordinate, out this.yCoordinate))
				this.xCoordinate = this.yCoordinate = 0;
		}

		/// <summary>
		/// Measured X-coordinate
		/// </summary>
		protected float xCoordinate;

		/// <summary>
		/// Measured Y-coordinate
		/// </summary>
		protected float yCoordinate;
	}
}
