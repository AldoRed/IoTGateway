﻿using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class for angle elements.
	/// </summary>
	public abstract class Angle : LayoutElement
	{
		private DoubleAttribute degrees;

		/// <summary>
		/// Abstract base class for distance elements.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Angle(Layout2DDocument Document, ILayoutElement Parent)
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

			this.degrees = new DoubleAttribute(Input, "degrees");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.degrees.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Angle Dest)
				Dest.degrees = this.degrees.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (!this.degrees.TryEvaluate(State.Session, out this.angle))
			{
				this.angle = 0;
				this.defined = false;
			}
		}

		/// <summary>
		/// Measured distance
		/// </summary>
		protected double angle;

	}
}
