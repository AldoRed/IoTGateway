﻿using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class for points.
	/// </summary>
	public abstract class Point : LayoutContainer
	{
		private LengthAttribute x;
		private LengthAttribute y;
		private StringAttribute _ref;

		/// <summary>
		/// Abstract base class for points.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Point(Layout2DDocument Document, ILayoutElement Parent)
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

	}
}
