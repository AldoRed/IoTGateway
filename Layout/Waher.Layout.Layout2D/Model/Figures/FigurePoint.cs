﻿using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// Abstract base class for figures based on a point.
	/// </summary>
	public abstract class FigurePoint : Figure
	{
		private LengthAttribute x;
		private LengthAttribute y;
		private StringAttribute _ref;

		/// <summary>
		/// Abstract base class for figures based on a point.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public FigurePoint(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
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

			if (Destination is FigurePoint Dest)
			{
				Dest.x = this.x.CopyIfNotPreset();
				Dest.y = this.y.CopyIfNotPreset();
				Dest._ref = this._ref.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (!this.IncludePoint(State, this.x, this.y, this._ref, out this.xCoordinate, out this.yCoordinate))
			{
				this.xCoordinate = 0;
				this.yCoordinate = 0;

				this.IncludePoint(this.xCoordinate, this.yCoordinate);
			}
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
