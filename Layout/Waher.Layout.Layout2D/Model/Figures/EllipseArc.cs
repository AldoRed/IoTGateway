﻿using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// An ellipse arc
	/// </summary>
	public class EllipseArc : Ellipse
	{
		private DoubleAttribute startAngleRadians;
		private DoubleAttribute startAngleDegrees;
		private DoubleAttribute endAngleRadians;
		private DoubleAttribute endAngleDegrees;
		private BooleanAttribute clockwise;

		/// <summary>
		/// An ellipse arc
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public EllipseArc(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "EllipseArc";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.startAngleRadians = new DoubleAttribute(Input, "startAngleRadians");
			this.startAngleDegrees = new DoubleAttribute(Input, "startAngleDegrees");
			this.endAngleRadians = new DoubleAttribute(Input, "endAngleRadians");
			this.endAngleDegrees = new DoubleAttribute(Input, "endAngleDegrees");
			this.clockwise = new BooleanAttribute(Input, "clockwise");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.startAngleRadians.Export(Output);
			this.startAngleDegrees.Export(Output);
			this.endAngleRadians.Export(Output);
			this.endAngleDegrees.Export(Output);
			this.clockwise.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new EllipseArc(Document, Parent);
		}
	}
}
